using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services
{
    public class JConverter
    {
        public static T JsonConver<T>(string line) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(line);
            }
            catch (JsonReaderException jrex)
            {
                throw jrex;
            }
            catch (JsonSerializationException jrex)
            {
                throw jrex;
            }
            catch (JsonWriterException jrex)
            {
                throw jrex;
            }
            catch (Exception jrex)
            {
                throw jrex;
            }
        }

        public static dynamic JsonConvertDynamic(string line)
        {
            try
            {
                return JsonConvert.DeserializeObject(line);
            }
            catch (JsonReaderException jrex)
            {
                throw jrex;
            }
            catch (JsonSerializationException jrex)
            {
                throw jrex;
            }
            catch (JsonWriterException jrex)
            {
                throw jrex;
            }
            catch (Exception jrex)
            {
                throw jrex;
            }
        }
    }
}
