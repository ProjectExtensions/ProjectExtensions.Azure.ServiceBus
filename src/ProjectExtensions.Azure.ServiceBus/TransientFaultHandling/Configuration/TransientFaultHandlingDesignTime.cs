//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Transient Fault Handling Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

namespace Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration
{
    internal static class TransientFaultHandlingDesignTime
    {
        public static class ViewModelTypeNames
        {
            public const string RetryPolicyConfigurationSettingsViewModel =
                "Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration.Design.ViewModel.RetryPolicyConfigurationSettingsViewModel, Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration";

            public const string DefaultElementConfigurationProperty =
                "Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration.Design.ViewModel.DefaultElementConfigurationProperty, Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration";

            public const string TimeSpanElementConfigurationProperty =
                "Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration.Design.ViewModel.TimeSpanElementConfigurationProperty, Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration";
        }

        public static class CommandTypeNames
        {
            public const string WellKnownRetryStrategyElementCollectionCommand = "Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration.Design.ViewModel.Commands.WellKnownRetryStrategyElementCollectionCommand, Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration";
        }

        /// <summary>
        /// Class that contains common editor types used by the designtime.
        /// </summary>
        public static class EditorTypes
        {
            /// <summary>
            /// Type name of the TimeSpanEditor class, declared in the Microsoft.Practices.EnterpriseLibrary.WindowsAzure.Autoscaling.Configuration Assembly.
            /// </summary>
            public const string TimeSpanEditor = "Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration.ComponentModel.Editors.TimeSpanEditorControl, Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration";

            /// <summary>
            /// Type name of the FrameworkElement, declared class in the PresentationFramework Assembly.
            /// </summary>
            public const string FrameworkElement = "System.Windows.FrameworkElement, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        }

        public static class ValidatorTypes
        {
            public const string NameValueCollectionValidator = "Microsoft.Practices.EnterpriseLibrary.Configuration.Design.Validation.NameValueCollectionValidator, Microsoft.Practices.EnterpriseLibrary.Configuration.DesignTime";

            public const string ExponentialBackoffValidator = "Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration.Validation.ExponentialBackoffValidator, Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Configuration";
        }
    }
}
