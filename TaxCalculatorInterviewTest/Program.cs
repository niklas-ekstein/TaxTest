using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

//The focus should be on clean, simple and easy to read code 
//Everything but the public interface may be changed
namespace TaxCalculatorInterviewTest
{
    class Program
    {
        public static List<TaxCalculator> ListTaxCalculators = new List<TaxCalculator>();

        static void Main(string[] args)
        {
            Start();
        }

        public static void Start()
        {
            Console.WriteLine("Choose y to create a new tax calculation or n to access an old? y/n");
            string svar = Console.ReadLine();
            svar.ToLower();

            if (svar == "y")
            {
                NewInstance();
            }
            else if (svar == "n")
            {
                OldInstance();
            }
            else
            {
                Start();
            }
        }

        public static void NewInstance()
        {
            Console.WriteLine("Enter your new tax ID:");
            string taxID = Console.ReadLine();
            TaxCalculator taxCalc = new TaxCalculator(taxID);

            ListTaxCalculators.Add(taxCalc);
            methodOptions(taxCalc);
        }

        public static void OldInstance()
        {
            Console.WriteLine("Enter the ID of your old tax calculator:");
            string answer = Console.ReadLine();

            try
            {
                TaxCalculator oldTaxCalc = ListTaxCalculators.First(x => x.Id == answer);
                methodOptions(oldTaxCalc);
            }
            catch
            {
                Console.WriteLine("Can't find ID");
                OldInstance();
            }
        }

        public static void methodOptions(TaxCalculator taxCalc)
        {
            Console.WriteLine("Press a number of what you would like to do:");
            Console.WriteLine("1. GetStandardTaxRate(Commodity commodity);");
            Console.WriteLine("2. SetCustomTaxRate(Commodity commodity, double rate);");
            Console.WriteLine("3. GetTaxRateForDateTime(Commodity commodity, DateTime date);");
            Console.WriteLine("4. GetCurrentTaxRate(Commodity commodity);");
            Console.WriteLine("5. End current tax calculation instance");

            string Answer = Console.ReadLine();

            chooseMethod(taxCalc, Answer);
        }

        public static void chooseMethod(TaxCalculator taxCalc, string answer)
        {
            if (answer == "1")
            {
                GetStandard(taxCalc);
            }
            if (answer == "2")
            {
                SetCustom(taxCalc);
            }
            if (answer == "3")
            {
                GetRateForDateTime(taxCalc);
            }
            if (answer == "4")
            {
                GetCurrentRate(taxCalc);
            }
            if (answer == "5")
            {
                Start();
            }
            else
            {
                Console.WriteLine("Answer 1, 2, 3, 4 or 5");
                methodOptions(taxCalc);
            }
        }

        //1. Get standard tax rate
        public static void GetStandard(TaxCalculator taxCalc)
        {
            Console.WriteLine("Write the commodity that you want standard tax rate for: " +
                "Alcohol, Food, FoodServices, Literature, Transport, CulturalServices");
            string commodityTax = Console.ReadLine();
            commodityTax = FormatString(commodityTax);

            double theRate = taxCalc.GetStandardTaxRate((Commodity)Enum.Parse(typeof(Commodity), commodityTax));

            Console.WriteLine(theRate.ToString());
        }

        //2. Set custom tax rate
        public static void SetCustom(TaxCalculator taxCalc)
        {
            Console.WriteLine("Write the commodity you want to set a tax rate for: " +
                "Alcohol, Food, FoodServices, Literature, Transport, CulturalServices");
            string commodity = Console.ReadLine();
            commodity = FormatString(commodity);

            Console.WriteLine("What rate do you want for: " + commodity);
            string answerRate = Console.ReadLine();
            double valueRate = Convert.ToDouble(answerRate);

            taxCalc.SetCustomTaxRate((Commodity)Enum.Parse(typeof(Commodity), commodity), valueRate);
            Console.WriteLine(valueRate + " is now the current rate for " + commodity);
        }

        //3. Get rate for a date
        public static void GetRateForDateTime(TaxCalculator taxCalc)
        {
            Console.WriteLine("Write the commodity you want to get a tax rate date from: " +
                "Alcohol, Food, FoodServices, Literature, Transport, CulturalServices");
            string commodity = Console.ReadLine();
            commodity = FormatString(commodity);

            Console.WriteLine("Write the date you want the rate from. Exampel format: 01/01/2022 06:32:00");
            string inString = Console.ReadLine();
            DateTime dateValue;

            if (DateTime.TryParse(inString, out dateValue))
            {
                double rate = taxCalc.GetTaxRateForDateTime((Commodity)Enum.Parse(typeof(Commodity), commodity), dateValue);
                Console.WriteLine("The rate for that date: " + rate.ToString());
                methodOptions(taxCalc);
            }
            else
            {
                Console.WriteLine("Unable to convert '{0}' to a date.", inString);
                GetRateForDateTime(taxCalc);
            }
        }

        //4. Get current tax rate
        public static void GetCurrentRate(TaxCalculator taxCalc)
        {
            Console.WriteLine("Write the commodity you want to get the current tax rate from: " +
                "Alcohol, Food, FoodServices, Literature, Transport, CulturalServices");
            string commodity = Console.ReadLine();
            commodity = FormatString(commodity);

            double currentRate = taxCalc.GetCurrentTaxRate((Commodity)Enum.Parse(typeof(Commodity), commodity));

            Console.WriteLine("Current rate is: " + currentRate);
        }

        //Format commodity string
        public static string FormatString(string comm)
        {
            return comm.Substring(0, 1).ToUpper() + comm.Substring(1).ToLower();
        }

    }

    /// <summary>
    /// This is the public inteface used by our client and may not be changed
    /// </summary>
    public interface ITaxCalculator
    {
        double GetStandardTaxRate(Commodity commodity);
        void SetCustomTaxRate(Commodity commodity, double rate);
        double GetTaxRateForDateTime(Commodity commodity, DateTime date);
        double GetCurrentTaxRate(Commodity commodity);
    }

    //Class to take care of custom tax rate data
    public class CustomTaxRates
    {
        public Commodity Commodity;
        public double Rate;
        public DateTime CreatedTime;

        public CustomTaxRates(Commodity commodity, double rate, DateTime createdTime)
        {
            Commodity = commodity;
            Rate = rate;
            CreatedTime = createdTime;
        }
    }

    /// <summary>
    /// Implements a tax calculator for our client.
    /// The calculator has a set of standard tax rates that are hard-coded in the class.
    /// It also allows our client to remotely set new, custom tax rates.
    /// Finally, it allows the fetching of tax rate information for a specific commodity and point in time.
    /// TODO: We know there are a few bugs in the code below, since the calculations look messed up every now and then.
    ///       There are also a number of things that have to be implemented.
    /// </summary>
    public class TaxCalculator : ITaxCalculator
    {
        public string Id;
        public List<Commodity> ListCurrentRate;
        public double? currentTaxRate;
        public CustomTaxRates customTaxRates;
        public List<CustomTaxRates> _customRatesList = new List<CustomTaxRates>();

        public TaxCalculator(string id)
        {
            Id = id;
            ListCurrentRate = new List<Commodity>();
        }

        /// <summary>
        /// Get the standard tax rate for a specific commodity.
        /// </summary>
        public double GetStandardTaxRate(Commodity commodity)
        {
            if (commodity == Commodity.Default)
                return 0.25;
            if (commodity == Commodity.Alcohol)
                return 0.25;
            if (commodity == Commodity.Food)
                return 0.12;
            if (commodity == Commodity.FoodServices)
                return 0.12;
            if (commodity == Commodity.Literature)
                return 0.6;
            if (commodity == Commodity.Transport)
                return 0.6;
            if (commodity == Commodity.CulturalServices)
                return 0.6;

            return 0.25;
        }

        /// <summary>
        /// This method allows the client to remotely set new custom tax rates.
        /// When they do, we save the commodity/rate information as well as the UTC timestamp of when it was done.
        /// NOTE: Each instance of this object supports a different set of custom rates, since we run one thread per customer.
        /// </summary>
        public void SetCustomTaxRate(Commodity commodity, double rate)
        {
            //TODO: support saving multiple custom rates for different combinations of Commodity/DateTime
            //TODO: make sure we never save duplicates, in case of e.g. clock resets, DST etc - overwrite old values if this happens
            //_customRates.Add(commodity, Tuple.Create(DateTime.Now, rate));
            customTaxRates = new CustomTaxRates(commodity, rate, DateTime.UtcNow);
            _customRatesList.Add(customTaxRates);
            currentTaxRate = customTaxRates.Rate;
        }

        /// <summary>
        /// Gets the tax rate that is active for a specific point in time (in UTC).
        /// A custom tax rate is seen as the currently active rate for a period from its starting timestamp until a new custom rate is set.
        /// If there is no custom tax rate for the specified date, use the standard tax rate.
        /// </summary>
        public double GetTaxRateForDateTime(Commodity commodity, DateTime date)
        {
            //Filter out the rate for a given date and time
            CustomTaxRates result = _customRatesList.Where(x => x.Commodity == commodity && x.CreatedTime < date)
                .OrderByDescending(x => x.CreatedTime)
                .FirstOrDefault();

            if (result is null)
            {
                double standardTax = GetStandardTaxRate(commodity);
                return standardTax;
            }
            else
            {
                return result.Rate;
            }
        }

        /// <summary>
        /// Gets the tax rate that is active for the current point in time.
        /// A custom tax rate is seen as the currently active rate for a period from its starting timestamp until a new custom rate is set.
        /// If there is no custom tax currently active, use the standard tax rate.
        /// </summary>
        public double GetCurrentTaxRate(Commodity commodity)
        {
            if (currentTaxRate is null)
            {
                double standardTax = GetStandardTaxRate(commodity);
                return standardTax;
            }
            else
            {
                return (double)currentTaxRate;
            }
        }
    }

    public enum Commodity
    {
        //PLEASE NOTE: THESE ARE THE ACTUAL TAX RATES THAT SHOULD APPLY, WE JUST GOT THEM FROM THE CLIENT!
        Default,            //25%
        Alcohol,            //25%
        Food,               //12%
        FoodServices,       //12%
        Literature,         //6%
        Transport,          //6%
        CulturalServices    //6%
    }
}

