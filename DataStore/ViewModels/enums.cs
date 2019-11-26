using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStore.ViewModels
{
    public enum Category
    {
        GENERAL_GOODS,
        PERISHABLES,
        BREAKABLES
    }

    public enum Type
    {
        EMAIL,
        SMS,
        OTP
    }

    public enum Types
    {
        OneOff,
        Continuous
    }

    public enum Zones
    {
        AREA_1,
        AREA_2
    }

    public enum subsidiary
    {
        Life = 1,
        General = 2
    }

    public enum TypeOfCover
    {
        Comprehensive = 1,
        Third_Party = 2,
        Third_Party_Fire_And_Theft = 3
    }

    public enum Platforms
    {
        ADAPT = 1,
        USSD = 2,
        WEBSITE = 3,
        MAX = 4,
        OTHERS = 5
    }

    public enum MealPlanCategory
    {
        GainWeight = 1,
        LoseWeight = 2,
        MaintainWeight = 3
    }

    public enum Preference
    {
        Poultry = 1,
        Meat = 2,
        Fish = 3,
        SeaFood = 4,
        Pork = 5
    }

    public enum DaysInWeeks
    {
        Monday = 1,
        Tuesday = 2,
        Wednessday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6,
        Sunday = 7
    }

    public enum MealType
    {
        BreakFast = 1,
        Lunch = 2,
        Dinner = 3
    }

    public enum GivenBirth
    {
        Yes = 1,
        No = 2,
        NotApplicable = 3
    }

    public enum MaritalStatus
    {
        Single = 1,
        Married = 2,
        IdRatherNotSay = 3
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public enum Frequency
    {
        Monthly = 1,
        Annually = 2,
        Quarterly = 3,
        Bi_Annually = 4
    }

    public enum PolicyType
    {
        EsusuShield = 1,
        CapitalBuilder = 2,
        LifeTimeHarvest = 3
    }

    public enum Car
    {
        Start = 1,
        Stop = 2
    }

    public enum TravelCategory
    {
        WorldWide = 1,
        Schenghen = 2,
        MiddleAndAsia = 3,
        Africa = 4,
        WorldWide2 = 5
    }

    //World Wide => 1
    // Schengen => 2
    // Middle East & Asia => 3
    // Africa => 4
}
