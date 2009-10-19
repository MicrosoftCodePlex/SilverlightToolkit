﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows.Browser;

namespace Microsoft.Silverlight.Testing.Html
{
    /// <summary>
    /// The base managed HTML control type.  In most cases, HtmlControl should 
    /// be used instead.
    /// </summary>
    public class HtmlControlBase
    {
        /// <summary>
        /// The prefix to use for any auto-generated ID value.
        /// </summary>
        private const string AutoGeneratedIdPrefix = "id_";

        /// <summary>
        /// The static, shared HtmlControlManager instance.
        /// </summary>
        private static HtmlControlManager _allControls = new HtmlControlManager();

        /// <summary>
        /// Used by an extension method on System.Windows.Browser.HtmlElement to
        /// hook a managed control up by creating a real underlying element.
        /// </summary>
        /// <param name="parent">The HtmlElement to append the managed 
        /// HtmlControl to.</param>
        /// <param name="control">The managed HtmlControlBase or HtmlControl to 
        /// append, Initialize, and create with a real element.</param>
        internal static void AppendAndInitialize(HtmlElement parent, HtmlControlBase control)
        {
            // If the control is already out there with live and active 
            // elements, the net effect of these calls is simply to initialize 
            // any children of the control.
            control.CreateAndAppend(parent);
            control.Initialize();
        }

        /// <summary>
        /// The current auto-generating Id index.
        /// </summary>
        private static int _autogeneratedIdIndex;

        /// <summary>
        /// The wrapper to save settings, events, and other properties on.
        /// </summary>
        private HtmlElementWrapper _wrapper;

        /// <summary>
        /// A collection of children controls.
        /// </summary>
        private HtmlControlCollection _controls;

        /// <summary>
        /// The parent managed HTML control reference.
        /// </summary>
        private HtmlControlBase _parent;

        /// <summary>
        /// The underlying, actual browser HtmlElement object.
        /// </summary>
        private HtmlElement _htmlElement;

        /// <summary>
        /// The tag name.
        /// </summary>
        private string _tag;

        /// <summary>
        /// Initialize the HtmlControlBase and its fields.
        /// </summary>
        private HtmlControlBase()
        {
            _wrapper = new HtmlElementWrapperBag();
            _controls = new HtmlControlCollection(this);
        }

        /// <summary>
        /// Initializes a new Control, places it inside the parent control, and 
        /// creates the underlying HtmlElement object.
        /// </summary>
        /// <param name="tag">The new type of tag name to create.</param>
        public HtmlControlBase(string tag) : this()
        {
            _tag = tag;
        }

        /// <summary>
        /// Initializes a new Control, places it inside the parent control, and 
        /// creates the underlying HtmlElement object.
        /// </summary>
        /// <param name="tag">The new type of tag name to create.</param>
        public HtmlControlBase(HtmlTag tag) : this(tag.ScriptPropertyName()) 
        { 
        }

        /// <summary>
        /// Initializes a new Control object, as the root (no parent).  The 
        /// HtmlElement needs to already exist.
        /// 
        /// Very important note:
        /// The Initialize method is not called automatically if you are using 
        /// this constructor - you must call Initialize if your code expects it 
        /// to be.
        /// </summary>
        /// <param name="element">The root HTML element.</param>
        public HtmlControlBase(HtmlElement element) : this()
        {
            SetHtmlElementInternal(element);
            _tag = element.TagName;

            SetParentIfExists(element.Parent);
            AppendSelfToParentControl();
            ConvertBagWrapper();
        }

        /// <summary>
        /// Creates the actual HtmlElement and prepares its parent 
        /// associations.
        /// </summary>
        /// <param name="parentElement">The parent, containing HtmlElement.</param>
        private void CreateAndAppend(HtmlElement parentElement)
        {
            if (parentElement == null)
            {
                throw new ArgumentNullException("parentElement");
            }
            if (_htmlElement == null)
            {
                HtmlElement element = HtmlPage.Document.CreateElement(_tag);
                SetHtmlElementInternal(element);
                parentElement.AppendChild(_htmlElement);
            }
            SetParentIfExists(parentElement);
            AppendSelfToParentControl();
        }

        /// <summary>
        /// Sets the managed HtmlControlBase parent, if it exists and is 
        /// registered, for an HtmlElement.
        /// </summary>
        /// <param name="parentHtmlElement">The parent HtmlElement.</param>
        private void SetParentIfExists(HtmlElement parentHtmlElement)
        {
            if (parentHtmlElement != null && _parent == null && _allControls.HasControl(parentHtmlElement))
            {
                _parent = _allControls.GetControl(parentHtmlElement);
            }
        }

        /// <summary>
        /// Ensures that the parent control, if any, has this control as a 
        /// child.
        /// </summary>
        private void AppendSelfToParentControl()
        {
            if (_parent != null && !_parent.Controls.Contains(this))
            {
                _parent.Controls.Add(this);
            }
        }

        /// <summary>
        /// Sets the actual HtmlElement for the control.
        /// </summary>
        /// <param name="element">The actual HtmlElement instance.</param>
        private void SetHtmlElementInternal(HtmlElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            _htmlElement = element;
            _allControls.RegisterControl(this);
        }

        /// <summary>
        /// Called to initialize the control.  This will create the real 
        /// HtmlElement on the page when appended.  This also initializes 
        /// sub-controls and can be overridden as necessary.
        /// </summary>
        public virtual void Initialize()
        {
            RequireInitialization();
            ConvertBagWrapper();
            
            // Append and initialize the child controls at the same time
            for (int i = 0; i < _controls.Count; ++i)
            {
                AppendAndInitialize(_htmlElement, _controls[i]);
            }
        }

        /// <summary>
        /// Converts from a property bag to a live wrapper.  If the current 
        /// wrapper is not a bag, then it creates a new wrapper around the live 
        /// HtmlElement.
        /// </summary>
        private void ConvertBagWrapper()
        {
            HtmlElementWrapperBag bag = _wrapper as HtmlElementWrapperBag;
            if (bag == null)
            {
                _wrapper = new HtmlElementWrapper(_htmlElement);
            }
            else
            {
                // Create the live, direct-wired wrapper to the element
                _wrapper = new HtmlElementWrapper(_htmlElement, bag);
            } 
        }

        /// <summary>
        /// Performs a check that initialization has happened.  Will throw an 
        /// InvalidOperationException if it has not yet initialized.
        /// </summary>
        protected void RequireInitialization()
        {
            if (_htmlElement == null)
            {
                throw new InvalidOperationException("The actual, initialized HTML element needs to exist for this operation to complete.");
            }
        }

        /// <summary>
        /// Gets the underlying HtmlElement object.
        /// </summary>
        public HtmlElement HtmlElement
        {
            get
            {
                return _htmlElement;
            }
        }

        /// <summary>
        /// Gets or sets the CSS class for the element.
        /// </summary>
        public string CssClass
        {
            get
            {
                string cssClass = GetProperty(HtmlProperty.ClassName) as string;
                if (cssClass == null)
                {
                    return String.Empty;
                }
                return cssClass;
            }
            set
            {
                SetProperty(HtmlProperty.ClassName, value ?? String.Empty);
            }
        }

        /// <summary>
        /// Gets the collection of child controls.
        /// </summary>
        public HtmlControlCollection Controls
        {
            get
            {
                return _controls;
            }
        }

        /// <summary>
        /// Checks if the control has managed child controls.
        /// </summary>
        /// <returns>Returns whether there are child controls.</returns>
        public bool HasControls()
        {
            return _controls.Count > 0;
        }

        /// <summary>
        /// Gets the dispatcher of the element.
        /// </summary>
        public System.Windows.Threading.Dispatcher Dispatcher
        {
            get
            {
                RequireInitialization();
                return HtmlElement.Dispatcher;
            }
        }

        /// <summary>
        /// Gets the parent managed Control.
        /// </summary>
        public HtmlControlBase Parent
        {
            get
            {
                return _parent;
            }
        }

        /// <summary>
        /// Gets or sets the control Id.  If the value is requested, but no Id 
        /// has been assigned, a new, special, unique Id is auto-generated and 
        /// assigned.
        /// </summary>
        public string Id
        {
            get
            {
                return GetProperty(HtmlProperty.Id) as string ?? GenerateId();
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                SetProperty(HtmlProperty.Id, value);
            }
        }

        /// <summary>
        /// Generates and assigns an ID to the control.  Also returns the value.
        /// </summary>
        /// <returns>Returns the generated and assigned ID.</returns>
        private string GenerateId()
        {
            ++_autogeneratedIdIndex;
            string id = AutoGeneratedIdPrefix + _autogeneratedIdIndex.ToString(CultureInfo.InvariantCulture);

            Id = id;
            return id;
        }

        /// <summary>
        /// Gets the tag name of the element.
        /// </summary>
        public string TagName
        {
            get
            {
                return _tag;
            }
        }

        /// <summary>
        /// Get a property value as an integer.
        /// </summary>
        /// <param name="property">The ScriptObject property name.</param>
        /// <returns>Returns the property value.</returns>
        protected int GetPropertyNumber(string property)
        {
            return Convert.ToInt32((double)GetProperty(property));
        }

        /// <summary>
        /// Get a property value as an integer.
        /// </summary>
        /// <param name="property">The ScriptObject property name.</param>
        /// <returns>Returns the property value.</returns>
        protected int GetPropertyNumber(HtmlProperty property)
        {
            return GetPropertyNumber(property.ScriptPropertyName());
        }

        #region Wrapper HtmlElement Methods

        /// <summary>
        /// Gets a property.
        /// </summary>
        /// <param name="property">The HTML property.</param>
        /// <returns>Returns the value.</returns>
        public object GetProperty(HtmlProperty property)
        {
            return GetProperty(property.ScriptPropertyName());
        }

        /// <summary>
        /// Sets a property.
        /// </summary>
        /// <param name="property">The HTML property.</param>
        /// <param name="value">The value to set.</param>
        public void SetProperty(HtmlProperty property, object value)
        {
            SetProperty(property.ScriptPropertyName(), value);
        }

        /// <summary>
        /// Attach an event handler to the element.
        /// </summary>
        /// <param name="eventName">The HTML script event name.</param>
        /// <param name="handler">The event handler.</param>
        public void AttachEvent(string eventName, EventHandler<HtmlEventArgs> handler)
        {
            _wrapper.AttachEvent(eventName, handler);
        }

        /// <summary>
        /// Attach an event handler to the element.
        /// </summary>
        /// <param name="eventName">The HTML script event name.</param>
        /// <param name="handler">The event handler.</param>
        public void AttachEvent(string eventName, EventHandler handler)
        {
            _wrapper.AttachEvent(eventName, handler);
        }

        /// <summary>
        /// Attach an event handler to the element.
        /// </summary>
        /// <param name="eventName">The HTML script event name.</param>
        /// <param name="handler">The event handler.</param>
        public void AttachEvent(HtmlEvent eventName, EventHandler handler)
        {
            AttachEvent(eventName.ScriptPropertyName(), handler);
        }

        /// <summary>
        /// Attach an event handler to the element.
        /// </summary>
        /// <param name="eventName">The HTML script event name.</param>
        /// <param name="handler">The event handler.</param>
        public void AttachEvent(HtmlEvent eventName, EventHandler<HtmlEventArgs> handler)
        {
            AttachEvent(eventName.ScriptPropertyName(), handler);
        }

        /// <summary>
        /// Get an attribute from the element.
        /// </summary>
        /// <param name="name">The attribute name to lookup.</param>
        /// <returns>Returns the attribute's value.</returns>
        public string GetAttribute(string name)
        {
            return _wrapper.GetAttribute(name);
        }

        /// <summary>
        /// Get an attribute from the element.
        /// </summary>
        /// <param name="attribute">The attribute name to lookup.</param>
        /// <returns>Returns the attribute's value.</returns>
        public string GetAttribute(HtmlAttribute attribute)
        {
            return GetAttribute(attribute.ScriptPropertyName());
        }

        /// <summary>
        /// Gets the style attribute.
        /// </summary>
        /// <param name="name">The attribute to get.</param>
        /// <returns>Returns the string value of the style attribute.</returns>
        public string GetStyleAttribute(string name)
        {
            return _wrapper.GetStyleAttribute(name);
        }

        /// <summary>
        /// Gets the style attribute.
        /// </summary>
        /// <param name="name">The attribute to get.</param>
        /// <returns>Returns the string value of the style attribute.</returns>
        public string GetStyleAttribute(CssAttribute name)
        {
            return GetStyleAttribute(name.ScriptPropertyName());
        }

        /// <summary>
        /// Remove an attribute from the element.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        public void RemoveAttribute(string name)
        {
            _wrapper.RemoveAttribute(name);
        }

        /// <summary>
        /// Remove an attribute from the element.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        public void RemoveAttribute(HtmlAttribute attribute)
        {
            RemoveAttribute(attribute.ScriptPropertyName());
        }

        /// <summary>
        /// Remove a style attribute from the element.
        /// </summary>
        /// <param name="name">The style attribute name.</param>
        public void RemoveStyleAttribute(string name)
        {
            _wrapper.RemoveStyleAttribute(name);
        }

        /// <summary>
        /// Remove a style attribute from the element.
        /// </summary>
        /// <param name="name">The style attribute name.</param>
        public void RemoveStyleAttribute(CssAttribute name)
        {
            RemoveStyleAttribute(name.ScriptPropertyName());
        }

        /// <summary>
        /// Set an attribute value.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The value to set.</param>
        public void SetAttribute(string name, string value)
        {
            _wrapper.SetAttribute(name, value);
        }

        /// <summary>
        /// Set an attribute value.
        /// </summary>
        /// <param name="attribute">The attribute name.</param>
        /// <param name="value">The value to set.</param>
        public void SetAttribute(HtmlAttribute attribute, string value)
        {
            SetAttribute(attribute.ScriptPropertyName(), value);
        }

        /// <summary>
        /// Sets the style attribute.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="value">The value to set the property to.</param>
        public void SetStyleAttribute(string name, string value)
        {
            _wrapper.SetStyleAttribute(name, value);
        }

        /// <summary>
        /// Sets the style attribute.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="value">The value to set the property to.</param>
        public void SetStyleAttribute(CssAttribute name, string value)
        {
            SetStyleAttribute(name.ScriptPropertyName(), value);
        }

        /// <summary>
        /// Sets the style attribute.
        /// </summary>
        /// <param name="name">Attribute name.</param>
        /// <param name="unit">The value to set the property to.</param>
        public void SetStyleAttribute(CssAttribute name, Unit unit)
        {
            SetStyleAttribute(name, unit.ToString());
        }

        /// <summary>
        /// Sets the style attribute.
        /// </summary>
        /// <param name="name">Attribute name.</param>
       /// <param name="enumValue">The value to set the property to.</param>
        public void SetStyleAttribute(CssAttribute name, Enum enumValue)
        {
            SetStyleAttribute(name, enumValue.ScriptPropertyName());
        }

        /// <summary>
        /// Gets the attribute value as a string, null if it has not been set.
        /// </summary>
        /// <param name="name">The attribute.</param>
        /// <returns>Returns the value.</returns>
        protected string GetAttributeOrNull(string name)
        {
            return GetAttribute(name) ?? String.Empty;
        }

        /// <summary>
        /// Gets the attribute value as a string, null if it has not been set.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns>Returns the value.</returns>
        protected string GetAttributeOrNull(HtmlAttribute attribute)
        {
            return GetAttributeOrNull(attribute.ScriptPropertyName());
        }

        /// <summary>
        /// Gets a property.
        /// </summary>
        /// <param name="name">The HTML property.</param>
        /// <returns>Returns the value.</returns>
        public object GetProperty(string name)
        {
            return _wrapper.GetProperty(name);
        }

        /// <summary>
        /// Sets a property.
        /// </summary>
        /// <param name="name">The HTML property.</param>
        /// <param name="value">The value to set.</param>
        public void SetProperty(string name, object value)
        {
            _wrapper.SetProperty(name, value);
        }

        #endregion
    }
}