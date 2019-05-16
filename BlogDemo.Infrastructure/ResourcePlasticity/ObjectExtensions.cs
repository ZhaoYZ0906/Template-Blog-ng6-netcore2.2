using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace BlogDemo.Infrastructure.ResourcePlasticity
{
    /// <summary>
    /// ������Դ����
    /// </summary>
    public static class ObjectExtensions
    {
        public static ExpandoObject ToDynamic<TSource>(this TSource source, string fields = null)
        {
            //�����Ҫ���εĶ���Ϊ�գ����׳��쳣
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            //�̳���IEnumerable��һ��ѽӿڣ�����һ��IEnumerable����
            var dataShapedObject = new ExpandoObject();
            //���fieldsΪ�� �򷵻����е�����
            if (string.IsNullOrWhiteSpace(fields))
            {
                //�����ȡ������
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                //����Ӧ��������ֵ��ӵ�dataShapedObject
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    //GetValue����source������� �� propertyInfo���Ե�ֵ
                    var propertyValue = propertyInfo.GetValue(source);
                    ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                }
                return dataShapedObject;
            }

            //���fields��Ϊ���򷵻�ָ��������
            var fieldsAfterSplit = fields.Split(',').ToList();

            //���������һ��
            foreach (var field in fieldsAfterSplit)
            {
                var propertyName = field.Trim();
                var propertyInfo = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null)
                {
                    throw new Exception($"Can't found property ��{typeof(TSource)}�� on ��{propertyName}��");
                }
                var propertyValue = propertyInfo.GetValue(source);
                ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
            }

            return dataShapedObject;
        }
    }
}
