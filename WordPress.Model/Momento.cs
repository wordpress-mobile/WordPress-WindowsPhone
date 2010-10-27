using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace WordPress.Model
{
    /// <summary>
    /// Captures a snapshot of the given object's public read + write properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Momento<T>
    {
        #region member variables

        private Dictionary<PropertyInfo, object> _state;
        private T _target;

        #endregion

        #region contructor

        public Momento(T target)
        {
            CaptureState(target);
        }

        #endregion

        #region methods

        private void CaptureState(T target)
        {
            if (null == target)
            {
                throw new ArgumentException("Target may not be null", "target");
            }
            
            _state = new Dictionary<PropertyInfo, object>();
            _target = target;

            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                    .Where(propertyInfo => propertyInfo.CanRead && propertyInfo.CanWrite && !propertyInfo.GetSetMethod(true).IsPrivate);
            foreach (PropertyInfo propertyInfo in properties)
            {
                _state[propertyInfo] = propertyInfo.GetValue(target, null);
            }
        }

        /// <summary>
        /// Restores the target's property values to the snapshot taken.
        /// </summary>
        /// <param name="target"></param>
        public void RestoreState(T target)
        {
            foreach (var pair in _state)
            {
                pair.Key.SetValue(target, pair.Value, null);
            }
        }

        #endregion
    }
}
