using System;
using System.IO;
using System.Linq;
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
                        <Supplier1>
                            <Input1 />
                            <Input2 />
                            <Input3 />
                        </Supplier1>
                        <Supplier2>
                            <Input1 />
                            <Input2 />
                            <Input3 />
                        </Supplier2>
                        <Supplier3>
                            <Input1 />
                            <Input2 />
                            <Input3 />
                        </Supplier3>
                        <Supplier4>
                            <Input1 />
                            <Input2 />
                            <Input3 />
                        </Supplier4>
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
                // Root
                writer.WriteLine("4,5,-1,1.0,1.00,0.00");
                    // Supplier1
                    writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                        // Input1
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                        // Input2
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                        // Input3
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                    // Supplier2
                    writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                        // Input1
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                        // Input2
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                        // Input3
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                    // Supplier3
                    writer.WriteLine("4,5,-1,1.0,0.25,0.05");
                        // Input1
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                        // Input2
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                        // Input3
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                    // Supplier4
                    writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                        // Input1
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                        // Input2
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
                        // Input3
                        writer.WriteLine("4,5,-1,1.0,0.25,0.00");
            }
            return new DelimitedFilePath(csv, ',');
        }
    }
}