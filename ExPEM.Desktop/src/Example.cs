using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AD.IO;
using AD.PartialEquilibriumApi;
using JetBrains.Annotations;

namespace ExPEM.Desktop
{
    public static class Example
    {
        public static XElement Example1()
        {
            XmlFilePath structureFile = CreateTempXmlFile();
            DelimitedFilePath dataFile = CreateTempCsvFile();

            // Read in the model and the data.
            XElement model = CreateModelFromFile(structureFile, dataFile);
            //XElement model = CreateModelFromInteractive(dataFile);

            // Set the consumer prices.
            model.SetConsumerPrices(model.DescendantsAndSelf()
                                        .Select(x => x.InitialPrice())
                                        .ToArray());

            // Apply the price shocks.
            model.ShockAllProducerPrices();

            // Calculate the price indices.
            model.CalculateConsumerPriceIndex();

            // Calculate the market equilibrium starting on the root.
            model.CalculateRootMarketEquilibrium();

            // Create boolean vector indicating which nodes (in document-order) are variable.
            bool[] variables =
                new bool[]
                {
                    false,
                    true,
                    true,
                    true,
                    true
                };

            // Create the objective function.
            Func<double[], double> objectiveFunction =
                x =>
                {
                    // Update consumer prices to the argument vector.
                    // Result: x[i] if variables[i] is true
                    model.SetConsumerPrices(x, variables);
                    // Shock the current prices:
                    // Result: currentPrice * (1 + shock)
                    model.ShockAllProducerPrices();
                    // Calculate a price index for the sector:
                    // Result: [Σ marketShare[i] * (price[i] ^ (1 - elasticityOfSubstitution[i])] ^ [1 / (1 - elasticityOfSubstitution)]
                    model.CalculateConsumerPriceIndex();
                    // Caclulate the market equilibrium. Zero means equilibrium.
                    // [shockedPrice ^ elasticityOfSupply] - [(priceIndex ^ (elasticityOfSubstitution + elasticityOfDemand)) / (initialPrice ^ elasticityOfSubstitution)]
                    model.CalculateRootMarketEquilibrium();
                    // Return the sector's equilibrium value to the caller.
                    return model.MarketEquilibrium();
                };

            // Set up the simplex solver.
            Simplex simplex =
                new Simplex(
                    objectiveFunction: x => objectiveFunction(x),
                    lowerBound: 0,
                    upperBound: 100,
                    dimensions: 5,
                    numberOfSolutions: 5,
                    iterations: 1000,
                    textWriter: Console.Out);

            // Find the minimum solution.
            Solution solution = simplex.Minimize();

            // Update the XML tree one more time with the optimal result.
            double[] result = solution.Vector;
            model.SetConsumerPrices(result, variables);
            model.ShockAllProducerPrices();
            model.CalculateConsumerPriceIndex();
            model.CalculateRootMarketEquilibrium();

            // Calculate new market shares
            model.CalculateAllFinalMarketShares();

            return model;

            // Print the results.
            Console.WriteLine("-----------------------------------------------------------------------------------------");
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                NewLineOnAttributes = true,
                IndentChars = "    "
            };
            using (XmlWriter writer = XmlWriter.Create(Console.Out, settings))
            {
                model.WriteTo(writer);
            }
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------------------------------------------------------");
            Console.ReadLine();
        }

        [UsedImplicitly]
        public static XElement CreateModelFromInteractive(DelimitedFilePath dataFile)
        {
            // Define the product market.
            XElement model = new XElement("Retail").DefineAttributeData(dataFile, 0);

            // Define the supplier markets and the initial prices of purchase from those markets.
            XElement supplier1 = new XElement("Supplier1").DefineAttributeData(dataFile, 1);
            XElement supplier2 = new XElement("Supplier2").DefineAttributeData(dataFile, 2);
            XElement supplier3 = new XElement("Supplier3").DefineAttributeData(dataFile, 3);
            XElement supplier4 = new XElement("Supplier4").DefineAttributeData(dataFile, 4);

            // Add the supplier markets to the product market. This has the effect of splitting the product market into product supplied by the supplier markets.
            model.Add(supplier1, supplier2, supplier3, supplier4);

            return model;
        }

        [UsedImplicitly]
        public static XElement CreateModelFromFile(XmlFilePath structureFile, DelimitedFilePath dataFile)
        {
            return XElement.Load(structureFile)
                           .DefineAttributeData(dataFile);
        }

        [UsedImplicitly]
        public static XmlFilePath CreateTempXmlFile()
        {
            string xml = Path.ChangeExtension(Path.GetTempFileName(), ".xml");
            using (StreamWriter writer = new StreamWriter(xml))
            {
                writer.WriteLine(
                    @"<Retail>
                        <Supplier1 />
                        <Supplier2 />
                        <Supplier3 />
                        <Supplier4 />
                      </Retail>");
            }
            return new XmlFilePath(xml);
        }

        [UsedImplicitly]
        public static DelimitedFilePath CreateTempCsvFile()
        {
            string csv = Path.ChangeExtension(Path.GetTempFileName(), ".csv");
            using (StreamWriter writer = new StreamWriter(csv))
            {
                writer.WriteLine("ElasticityOfSubstitution,ElasticityOfSupply,ElasticityOfDemand,InitialPrice,InitialMarketShare,Shock");
                writer.WriteLine("4,5,-1,1.0,1.00,0.00");
                writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                writer.WriteLine("4,5,-1,1.0,0.25,0.05");
                writer.WriteLine("4,5,-1,1.0,0.25,0.00");
            }
            return new DelimitedFilePath(csv, ',');
        }
    }
}