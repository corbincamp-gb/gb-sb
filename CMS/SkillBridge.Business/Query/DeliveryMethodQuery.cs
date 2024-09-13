using SkillBridge.Business.Model.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taku.Core;

namespace SkillBridge.Business.Query
{
    public interface IDeliveryMethodQuery : IQuery
    {
        string Get(int deliveryMethod);
        string Get(string deliveryMethod);
    }

    public class DeliveryMethodQuery : IDeliveryMethodQuery
    {
        public string Get(string deliveryMethod)
        {
            return Get(int.Parse(deliveryMethod));
        }

        public string Get(int deliveryMethod)
        {
            var dms = new Dictionary<int, string>
            {
                { 0, "In-Person" },
                { 1, "In-Person" },
                { 2, "Online" },
                { 3, "Hybrid (In-Person and Online)" }
            };

            dms.TryGetValue(deliveryMethod, out var ret);

            return ret;

        }
    }
}
