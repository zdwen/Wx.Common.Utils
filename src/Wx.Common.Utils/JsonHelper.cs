using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Wx.Common.Utils.Modles.Entities.BOs.WxJsonConverter;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Wx.Common.Utils
{
    public class JsonHelper
    {
        static JsonSerializerSettings _settings;

        static JsonHelper()
        {
            _settings = new JsonSerializerSettings()
            {
                Converters = new JsonConverter[] 
                { 
                    new EnumJsonConverter(),
                    new IsoDateTimeConverter(),
                },
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
        }

        /// <summary>
        /// 【闻祖东 2014-8-8-160920】当前这个Json序列化方法可以满足对象在Json序列化时：
        /// 1.属性的首字母小写(如果属性有JsonProperty这个Attribute修饰，那么会以具体的Attribute指定的输出字段为优先)。
        /// 2.时间属性的标准ISO 8601 时间格式： (e.g. 2008-04-12T12:53Z)；
        /// 3.枚举对象属性直接输出为字符串的值；
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string JsonSerialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }

        public static T JsonDeserialize<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString, _settings);
        }
    }
}
