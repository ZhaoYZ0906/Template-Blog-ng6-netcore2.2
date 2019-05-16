using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace BlogDemo.Infrastructure.ResourcePlasticity
{
    /// <summary>
    /// 单个资源塑形
    /// </summary>
    public static class ObjectExtensions
    {
        public static ExpandoObject ToDynamic<TSource>(this TSource source, string fields = null)
        {
            //如果需要塑形的对象为空，则抛出异常
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            //继承于IEnumerable和一大堆接口，看作一个IEnumerable就行
            var dataShapedObject = new ExpandoObject();
            //如果fields为空 则返回所有的属性
            if (string.IsNullOrWhiteSpace(fields))
            {
                //反射读取属性名
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                //将对应属性名和值添加到dataShapedObject
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    //GetValue返回source这个对象 的 propertyInfo属性的值
                    var propertyValue = propertyInfo.GetValue(source);
                    ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                }
                return dataShapedObject;
            }

            //如果fields不为空则返回指定的属性
            var fieldsAfterSplit = fields.Split(',').ToList();

            //和上面基本一致
            foreach (var field in fieldsAfterSplit)
            {
                var propertyName = field.Trim();
                var propertyInfo = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    throw new Exception($"Can't found property ‘{typeof(TSource)}’ on ‘{propertyName}’");
                }
                var propertyValue = propertyInfo.GetValue(source);
                ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
            }

            return dataShapedObject;
        }
    }
}
