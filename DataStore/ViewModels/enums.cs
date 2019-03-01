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
        SMS
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
}
