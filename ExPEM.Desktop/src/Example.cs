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

            XName[] variables = 
                new XName[]
                {
                    "C",
                    "D",
                    "E"
                };

            // Read in the model and the data.
            XElement model =
                XElement.Load(structureFile)
                        .DefineAttributeData(dataFile)
                        .SetIsVariable(variables);

            // Create the objective function.
            Func<double[], double> objectiveFunction =
                x =>
                {
                    XElement localModel = new XElement(model);
                    localModel.SetConsumerPrices(x)
                              .ShockProducerPrices()
                              .CalculateMarketEquilibrium()
                              .CalculateFinalMarketShares();

                    return localModel.DescendantsAndSelf().Sum(y => Math.Pow(y.MarketEquilibrium(), 2));
                };

            // Set up the simplex solver.
            Simplex simplex =
                new Simplex(
                    objectiveFunction: x => objectiveFunction(x),
                    lowerBound: 0,
                    upperBound: 10,
                    dimensions: variables.Length,
                    iterations: 1000,
                    seed: 0,
                    textWriter: Console.Out
                );

            // Find the minimum solution.
            Solution solution = simplex.Minimize();

            // Apply the final solution
            model.SetConsumerPrices(solution.Vector)
                 .ShockProducerPrices()
                 .CalculateMarketEquilibrium()
                 .CalculateFinalMarketShares();

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
                    @"<A>
                        <B>
                            <C/>
                            <D/>
                            <E/>
                        </B>
                        <F>
                            <G/>
                            <H/>
                            <I/>
                        </F>
                        <J>
                            <K/>
                            <L/>
                            <M/>
                        </J>
                        <N>
                            <O/>
                            <P/>
                            <Q/>
                        </N>
                      </A>");
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