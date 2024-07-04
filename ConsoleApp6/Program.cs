using System;


try
{
    var employee = new InsuranceCalculator.Employee
    {
        AdditionDate = new DateTime(2024, 3, 1),
        PolicyEndDate = new DateTime(2024, 12, 31),
        Age = 25,
        EmployeeGender = InsuranceCalculator.Gender.Female
    };
    var result = InsuranceCalculator.CalculatePremium(employee, InsuranceCalculator.PricingModel.GenderAgeRated, InsuranceCalculator.ProrateMethod.ByMonths);
    Console.WriteLine($"Full Premium: {result.FullPremium}, Prorated Premium: {result.ProratedPremium}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

public class InsuranceCalculator
{
    #region Field
    
    private const decimal FLAT_RATE_PREMIUM = 1000;
    private const int MONTHS_IN_YEAR = 12;
    private const decimal FEMALE_MULTIPLIER = 1.5m;
    private const int FEMALE_MULTIPLIER_AGE_RULE = 18;
    private static readonly Dictionary<int, decimal> agePremiums = new Dictionary<int, decimal>
    {
        { 9, 100 },
        { 19, 200 },
        { 29, 300 },
        { 39, 400 },
        { 49, 500 },
        { 59, 600 },
        { 69, 700 },
        { 79, 800 },
        { 89, 900 },
        { 99, 1000 }
    };

    #endregion

    #region Methods
    public static (decimal FullPremium, decimal ProratedPremium) CalculatePremium(Employee employee, PricingModel pricingModel, ProrateMethod prorateMethod)
    {
        if (employee.AdditionDate > employee.PolicyEndDate)
        {
            throw new ArgumentException("AdditionDate cannot be after PolicyEndDate.");
        }

        decimal fullPremium = 0;

        // Calculate full premium based on pricing model
        switch (pricingModel)
        {
            case PricingModel.FlatRate:
                fullPremium = FLAT_RATE_PREMIUM;
                break;

            case PricingModel.AgeRated:
                fullPremium = GetAgeRatedPremium(employee.Age);
                break;

            case PricingModel.GenderAgeRated:
                fullPremium = GetGenderAgeRatedPremium(employee.Age, employee.EmployeeGender);
                break;
        }

        // Calculate prorated premium
        decimal proratedPremium = 0;
        switch (prorateMethod)
        {
            case ProrateMethod.ByDays:
                int remainingDays = (employee.PolicyEndDate - employee.AdditionDate).Days + 1;// AdditionDate.Day must also be included (+1)
                proratedPremium = fullPremium / GetDaysInYear(employee.PolicyEndDate.Year) * remainingDays;
                break;

            case ProrateMethod.ByMonths:
                int remainingMonths = employee.PolicyEndDate.Month - employee.AdditionDate.Month + 1; // AdditionDate.Month must also be included (+1)
                proratedPremium = fullPremium / MONTHS_IN_YEAR * remainingMonths;
                break;
        }

        return (fullPremium, proratedPremium);
    }

    private static decimal GetAgeRatedPremium(int age)
    {
        foreach (var premium in agePremiums)
        {
            if (age <= premium.Key)
                return age * premium.Value;
        }
        return 0; // Default case
    }

    private static decimal GetGenderAgeRatedPremium(int age, Gender gender)
    {
        decimal basePremium = GetAgeRatedPremium(age);

        if (gender == Gender.Female && age > FEMALE_MULTIPLIER_AGE_RULE)
        {
            basePremium *= FEMALE_MULTIPLIER;
        }

        return basePremium;
    }
    #endregion


    public static int GetDaysInYear(int year)
    {
        return DateTime.IsLeapYear(year) ? 366 : 365;
    }

    public enum PricingModel
    {
        FlatRate,
        AgeRated,
        GenderAgeRated
    }

    public enum ProrateMethod
    {
        ByDays,
        ByMonths
    }

    public enum Gender
    {
        Male,
        Female
    }

    public class Employee
    {
        private DateTime _additionDate;
        private DateTime _policyEndDate;
        public DateTime AdditionDate
        {
            get => _additionDate;
            set
            {
                if (!IsValidDate(value))
                {
                    throw new ArgumentException("AdditionDate is invalid.");
                }
                _additionDate = value;
            }
        }

        public DateTime PolicyEndDate
        {
            get => _policyEndDate;
            set
            {
                if (!IsValidDate(value))
                {
                    throw new ArgumentException("PolicyEndDate is invalid.");
                }
                _policyEndDate = value;
            }
        }
        public int Age { get; set; }
        public Gender EmployeeGender { get; set; }

        private bool IsValidDate(DateTime date)
        {
            return date.Month >= 1 && date.Month <= 12 && date.Day >= 1 && date.Day <= DateTime.DaysInMonth(date.Year, date.Month);
        }
    }
}

