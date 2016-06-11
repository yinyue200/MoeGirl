using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yfxsApp.runtime
{
    static class Version
    {
        public static string GetThisAppVersionString()
        {
            var a = Windows.ApplicationModel.Package.Current.Id.Version;
            return a.Major.ToString() + "." + a.Minor.ToString() + "." + a.Build.ToString() + "." + a.Revision.ToString();
        }
        public static string hcversion = null;
        public static async Task<string> GetWindowsVersionAsync()
        {
            if(hcversion!=null)
            {
                return hcversion;
            }
            try
            {
                string str;
                string[] strArray = { "{A8B865DD-2E3D-4094-AD97-E593A70C75D6},3" };
                Windows.Devices.Enumeration.Pnp.PnpObject halDevice = await GetHalDevice(strArray);
                if (halDevice == null || !halDevice.Properties.ContainsKey("{A8B865DD-2E3D-4094-AD97-E593A70C75D6},3"))
                    str = null;
                else
                    str = halDevice.Properties["{A8B865DD-2E3D-4094-AD97-E593A70C75D6},3"].ToString();
                hcversion = str;
                return str;
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        private static async Task<Windows.Devices.Enumeration.Pnp.PnpObject> GetHalDevice(params string[] properties)
        {
            string[] strArray = properties;
            string[] strArray1 = { "{A45C254E-DF1C-4EFD-8020-67D146A850E0},10" };
            IEnumerable<string> enumerable = strArray.Concat(strArray1);
            Windows.Devices.Enumeration.Pnp.PnpObjectCollection pnpObjectCollection = await Windows.Devices.Enumeration.Pnp.PnpObject.FindAllAsync(Windows.Devices.Enumeration.Pnp.PnpObjectType.Device, enumerable, "System.Devices.ContainerId:=\"{00000000-0000-0000-FFFF-FFFFFFFFFFFF}\"");

            foreach (Windows.Devices.Enumeration.Pnp.PnpObject pnpObject in pnpObjectCollection)
            {
                if (pnpObject.Properties == null || !pnpObject.Properties.Any())
                    continue;

                KeyValuePair<string, object> keyValuePair = pnpObject.Properties.Last();
                if (keyValuePair.Value == null || !keyValuePair.Value.ToString().Equals("4d36e966-e325-11ce-bfc1-08002be10318"))
                    continue;
                return pnpObject;
            }
            return null;
        }
    }
}
