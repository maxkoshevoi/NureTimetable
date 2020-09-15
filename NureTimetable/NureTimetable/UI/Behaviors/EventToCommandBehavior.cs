﻿using System;
using System.Reflection;
using System.Windows.Input;
using NureTimetable.UI.Behaviors.Base;
using Xamarin.Forms;

namespace NureTimetable.UI.Behaviors
{
    public class EventToCommandBehavior : BehaviorBase<VisualElement>
    {
        private Delegate _eventHandler;

        public static readonly BindableProperty EventNameProperty 
            = BindableProperty.Create(nameof(EventName), typeof(string), typeof(EventToCommandBehavior), null, propertyChanged: OnEventNameChanged);

        public static readonly BindableProperty CommandProperty 
            = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(EventToCommandBehavior));

        public static readonly BindableProperty CommandParameterProperty 
            = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(EventToCommandBehavior));

        public static readonly BindableProperty InputConverterProperty 
            = BindableProperty.Create(nameof(Converter), typeof(IValueConverter), typeof(EventToCommandBehavior));

        public string EventName
        {
            get => GetValue(EventNameProperty) as string;
            set => SetValue(EventNameProperty, value);
        }

        public ICommand Command
        {
            get => GetValue(CommandProperty) as ICommand;
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public IValueConverter Converter
        {
            get => GetValue(InputConverterProperty) as IValueConverter;
            set => SetValue(InputConverterProperty, value);
        }

        protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);
            RegisterEvent(EventName);
        }

        protected override void OnDetachingFrom(VisualElement bindable)
        {
            DeregisterEvent(EventName);
            base.OnDetachingFrom(bindable);
        }

        private void RegisterEvent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            var eventInfo = AssociatedObject.GetType().GetRuntimeEvent(name);

            if (eventInfo is null)
                throw new ArgumentException($"EventToCommandBehavior: Can't register the '{EventName}' event.");

            var methodInfo = typeof(EventToCommandBehavior).GetTypeInfo().GetDeclaredMethod(nameof(OnEvent));

            _eventHandler = methodInfo.CreateDelegate(eventInfo.EventHandlerType, this);

            eventInfo.AddEventHandler(AssociatedObject, _eventHandler);
        }

        private void DeregisterEvent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            if (_eventHandler is null)
                return;

            var eventInfo = AssociatedObject.GetType().GetRuntimeEvent(name);

            if (eventInfo is null)
                throw new ArgumentException($"EventToCommandBehavior: Can't de-register the '{EventName}' event.");

            eventInfo.RemoveEventHandler(AssociatedObject, _eventHandler);

            _eventHandler = null;
        }

        public void OnEvent(object sender, object eventArgs)
        {
            if (Command is null)
            {
                return;
            }

            object resolvedParameter;
            if (CommandParameter != null)
            {
                resolvedParameter = CommandParameter;
            }
            else if (Converter != null)
            {
                resolvedParameter = Converter.Convert(eventArgs, typeof(object), null, null);
            }
            else
            {
                resolvedParameter = eventArgs;
            }

            if (Command.CanExecute(resolvedParameter))
            {
                Command.Execute(resolvedParameter);
            }
        }

        private static void OnEventNameChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var behavior = (EventToCommandBehavior)bindable;

            if (behavior.AssociatedObject is null)
                return;

            var oldEventName = (string)oldValue;
            var newEventName = (string)newValue;

            behavior.DeregisterEvent(oldEventName);
            behavior.RegisterEvent(newEventName);
        }
    }
}