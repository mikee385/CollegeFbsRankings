using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Infrastructure.Csv
{
    public class CsvRepositoryConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("seasons", IsRequired = true)]
        public SeasonConfigurationCollection Seasons
        {
            get { return (SeasonConfigurationCollection)base["seasons"]; }
            set { base["seasons"] = value; }
        }
    }

    public class SeasonConfigurationCollection : ConfigurationElementCollection, IEnumerable<SeasonConfigurationElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SeasonConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SeasonConfigurationElement)element).Year;
        }

        public SeasonConfigurationElement this[int index]
        {
            get { return (SeasonConfigurationElement) BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new IEnumerator<SeasonConfigurationElement> GetEnumerator()
        {
            return this.OfType<SeasonConfigurationElement>().GetEnumerator();
        }
    }

    public class SeasonConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("year", IsRequired = true, IsKey = true)]
        public int Year
        {
            get { return (int)this["year"]; }
            set { this["year"] = value; }
        }

        [ConfigurationProperty("numWeeksInRegularSeason", IsRequired = true)]
        public int NumWeeksInRegularSeason
        {
            get { return (int)this["numWeeksInRegularSeason"]; }
            set { this["numWeeksInRegularSeason"] = value; }
        }

        [ConfigurationProperty("fbsTeamFile", IsRequired = true)]
        public string FbsTeamFile
        {
            get { return (string)this["fbsTeamFile"]; }
            set { this["fbsTeamFile"] = value; }
        }

        [ConfigurationProperty("fbsGameFile", IsRequired = true)]
        public string FbsGameFile
        {
            get { return (string)this["fbsGameFile"]; }
            set { this["fbsGameFile"] = value; }
        }
    }
}
