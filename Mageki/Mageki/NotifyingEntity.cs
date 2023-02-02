using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
//https://blog.csdn.net/weixin_43809174/article/details/118156700
namespace Mageki
{
    public abstract class NotifyingEntity : INotifyPropertyChanging, INotifyPropertyChanged
    {
        //储存属性值的字典
        private Dictionary<string, object> property = new Dictionary<string, object>();
        /// <summary>
        /// 设置值时触发通知事件
        /// </summary>
        /// <param name="value">需要设置的值</param>
        /// <param name="propertyName">CallerMemberName属性可以获取调用方的名称（不需要手动设置）</param>
        protected void SetValueWithNotify(object value, [CallerMemberName] string propertyName = "")
        {
            NotifyChanging(propertyName);
            if (property.ContainsKey(propertyName))
            {
                property[propertyName] = value;
            }
            else
            {
                property.Add(propertyName, value);
            }
            NotifyChanged(propertyName);
        }
        /// <summary>
        /// 获得对应的值
        /// </summary>
        /// <typeparam name="T">需要转换的类型</typeparam>
        /// <param name="propertyName">CallerMemberName属性可以获取调用方的名称（不需要手动设置）</param>
        protected T GetValue<T>(T defaultValue,[CallerMemberName] string propertyName = "")
            => property.ContainsKey(propertyName) ? (T)property[propertyName] : defaultValue;

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected void NotifyChanging(string propertyName)
            => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }
}
