using System;
using System.Collections.Generic;
using System.Text;
using activities_wpf.DataModel;
using CsvHelper.Configuration;

namespace activities_wpf.Mapper
{
    public sealed class FitnessActivityMapper : ClassMap<FitnessActivity>
    {
        public FitnessActivityMapper()
        {
            Map(m => m.DateStartTime).Index(0);
            Map(m => m.Title).Index(1);
            Map(m => m.Cost).Index(2);
            Map(m => m.Location).Index(3);
        }
    }
}