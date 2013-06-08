using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using Windows.UI.Popups;

namespace MetroExplorer.Helper
{
    public class XmlSerializationHelper
    {
        /// <summary>
        /// 把对象序列化为XML 文件
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="t"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task XMLSerialize<T>(T t, Stream stream)
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));

                await Task.Run(() =>
                {
                    xml.Serialize(stream, t);
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// 把 XML 文件反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<T> XMLDeserialize<T>(Stream stream) where T : class,new()
        {
            try
            {
                XmlSerializer xml = new XmlSerializer(typeof(T));
                object temp = null;
                await Task.Run(() =>
                {
                    temp = xml.Deserialize(stream);
                });
                return temp as T;
            }
            catch (System.InvalidOperationException)
            {
                return new T();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
