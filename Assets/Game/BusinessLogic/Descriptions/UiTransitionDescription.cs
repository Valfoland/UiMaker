using System;
using Frameworks.BlFramework.Base;
using Frameworks.BlFramework.Utils;
using Game.UiController.Transitions;
using Game.UiController.Windows;

namespace Game.BusinessLogic.Descriptions
{
    public class UiTransitionDescription : DescriptionBase
    {
        private const string TransitionTypeKey = "TransitionType";
        private const string DestinationWindowKey = "DestinationWindow";
        
        public TransitionType TransitionType { get; }
        public WindowType DestinationWindow { get; }
        
        public UiTransitionDescription(TreeData nodeData) : base(nodeData)
        {
            if (Enum.TryParse(nodeData.GetString(TransitionTypeKey), out TransitionType transitionType))
            {
                TransitionType = transitionType;
            }
            
            if (Enum.TryParse(nodeData.GetString(DestinationWindowKey), out WindowType destinationWindow))
            {
                DestinationWindow = destinationWindow;
            }
        }
    }
}
