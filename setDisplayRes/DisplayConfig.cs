using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration; //settings from app.config.

namespace setDisplayRes
{
    class DisplayConfig : ConfigurationSection
    {
        public List<DisplayElement> GetAllDisplays()
        {
            List<DisplayElement> displays = new List<DisplayElement>();

            foreach (DisplayElement item in configDisplays)
            {
                displays.Add(item);
            }

            return displays;
        }

        [ConfigurationProperty("Displays")]
        [ConfigurationCollection(typeof(DisplayElement), AddItemName = "Display")]
        protected DisplayCollection configDisplays
        {
            get { return ((DisplayCollection)(base["Displays"])); }
        }
    }//class

    

    [ConfigurationCollection(typeof(DisplayElement))]
    class DisplayCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DisplayElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DisplayElement)(element)).ID;
        }


        public DisplayElement this[int idx]
        {
            get
            {
                return (DisplayElement)BaseGet(idx);
            }
        }
    }//class

    public class DisplayElement : ConfigurationElement
    {
        public override string ToString()
        {
            String strValues = "";
            strValues += "name: '" + name + "' ";
            strValues += "setres: '" + setres.ToString() + "' ";
            strValues += "width: '" + width + "' ";
            strValues += "height: '" + height + "' ";
            strValues += "freqhz: '" + freqhz + "' ";   
            return strValues;
        }


        public DisplayElement()
            : base()
        {
            if (String.IsNullOrEmpty(ID))
            {
                ID = Guid.NewGuid().ToString();
            }
        }

        //constr set GUID when not in config
        [ConfigurationProperty("ID", DefaultValue = "", IsKey = true, IsRequired = false)]
        public string ID
        {
            get
            {
                return ((string)(base["ID"]));
            }

            set
            {
                base["ID"] = value;
            }

        }//ID


        [ConfigurationProperty("name", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string name
        {
            get
            {
                return ((string)(base["name"]));
            }

            set
            {
                base["name"] = value;
            }

        }

        [ConfigurationProperty("devicestring", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string devicestring
        {
            get
            {
                return ((string)(base["devicestring"]));
            }

            set
            {
                base["devicestring"] = value;
            }

        }

        [ConfigurationProperty("deviceid", DefaultValue = "", IsKey = false, IsRequired = false)]
        public string deviceid
        {
            get
            {
                return ((string)(base["deviceid"]));
            }

            set
            {
                base["deviceid"] = value;
            }

        }

        [ConfigurationProperty("setres", DefaultValue = "", IsKey = false, IsRequired = true)]
        public string setres
        {
            get
            {
                return ((string)(base["setres"]));
            }

            set
            {
                base["setres"] = value;
            }

        }
        

        [ConfigurationProperty("width", DefaultValue = 1280, IsKey = false, IsRequired = true)]
        public int width
        {
            get
            {
                return (int)(this["width"]);
            }

            set
            {
                base["width"] = value;
            }

        }


        [ConfigurationProperty("height", DefaultValue = 1024, IsKey = false, IsRequired = true)]
        public int height
        {
            get
            {
                return (int)this["height"];
            }

            set
            {
                base["height"] = value;
            }

        }

        [ConfigurationProperty("freqhz", DefaultValue = 0, IsKey = false, IsRequired = false)]
        public int freqhz
        {
            get
            {
                return System.Convert.ToInt32(base["freqhz"]);
            }

            set
            {
                base["freqhz"] = value.ToString();
            }

        }

        [ConfigurationProperty("primary", DefaultValue = false, IsKey = false, IsRequired = false)]
        public bool primary
        {
            get
            {
                return ((bool)(base["primary"]));
            }

            set
            {
                base["primary"] = value;
            }

        }//hidden



    }//class DisplayElement

}//ns
