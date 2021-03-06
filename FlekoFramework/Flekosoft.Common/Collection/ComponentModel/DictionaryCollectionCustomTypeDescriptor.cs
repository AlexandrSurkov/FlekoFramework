﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Flekosoft.Common.Collection.ComponentModel;

// ReSharper disable once CheckNamespace
namespace Flekosoft.Common.Collection
{
    public partial class DictionaryCollection<TK, TV>
    {
        #region TypeDesctiptor

        public String GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public String GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(TypeDescriptor.GetProperties(this));
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties(TypeDescriptor.GetProperties(this, attributes, true));
        }

        public virtual PropertyDescriptorCollection GetProperties(PropertyDescriptorCollection pds)
        {
            var coll = new PropertyDescriptorCollection(null);

            foreach (PropertyDescriptor pd in pds)
            {
                coll.Add(pd);
            }

            foreach (KeyValuePair<TK, TV> keyValuePair in InternalCollection)
            {
                var pd = new DictionaryCollectionItemPropertyDescription<TK, TV>(this, keyValuePair.Key);
                coll.Add(pd);
            }
            return coll;
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
    }
}
