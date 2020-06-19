﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.8009
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

// 
// This source code was auto-generated by wsdl, Version=2.0.50727.3038.
// 

namespace Roster.Presentation.Controls.NAVproxy
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.3038")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name = "ProcessTimeSheets_Binding", Namespace = "urn:microsoft-dynamics-schemas/codeunit/ProcessTimeSheets")]
    public partial class ProcessTimeSheets : System.Web.Services.Protocols.SoapHttpClientProtocol
    {

        private System.Threading.SendOrPostCallback ProcessTimeSheetOperationCompleted;

        /// <remarks/>
        public ProcessTimeSheets()
        {
            this.Url = "http://localhost:7047/TEST/WS/Koomarri/Codeunit/ProcessTimeSheets";
        }

        /// <remarks/>
        public event ProcessTimeSheetCompletedEventHandler ProcessTimeSheetCompleted;

        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("urn:microsoft-dynamics-schemas/codeunit/ProcessTimeSheets:ProcessTimeSheet", RequestNamespace = "urn:microsoft-dynamics-schemas/codeunit/ProcessTimeSheets", ResponseElementName = "ProcessTimeSheet_Result", ResponseNamespace = "urn:microsoft-dynamics-schemas/codeunit/ProcessTimeSheets", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void ProcessTimeSheet(string externalTimeSheetNo, bool submitTimeSheet)
        {
            this.Invoke("ProcessTimeSheet", new object[] {
                    externalTimeSheetNo,
                    submitTimeSheet});
        }

        /// <remarks/>
        public System.IAsyncResult BeginProcessTimeSheet(string externalTimeSheetNo, bool submitTimeSheet, System.AsyncCallback callback, object asyncState)
        {
            return this.BeginInvoke("ProcessTimeSheet", new object[] {
                    externalTimeSheetNo,
                    submitTimeSheet}, callback, asyncState);
        }

        /// <remarks/>
        public void EndProcessTimeSheet(System.IAsyncResult asyncResult)
        {
            this.EndInvoke(asyncResult);
        }

        /// <remarks/>
        public void ProcessTimeSheetAsync(string externalTimeSheetNo, bool submitTimeSheet)
        {
            this.ProcessTimeSheetAsync(externalTimeSheetNo, submitTimeSheet, null);
        }

        /// <remarks/>
        public void ProcessTimeSheetAsync(string externalTimeSheetNo, bool submitTimeSheet, object userState)
        {
            if ((this.ProcessTimeSheetOperationCompleted == null))
            {
                this.ProcessTimeSheetOperationCompleted = new System.Threading.SendOrPostCallback(this.OnProcessTimeSheetOperationCompleted);
            }
            this.InvokeAsync("ProcessTimeSheet", new object[] {
                    externalTimeSheetNo,
                    submitTimeSheet}, this.ProcessTimeSheetOperationCompleted, userState);
        }

        private void OnProcessTimeSheetOperationCompleted(object arg)
        {
            if ((this.ProcessTimeSheetCompleted != null))
            {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ProcessTimeSheetCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }

        /// <remarks/>
        public new void CancelAsync(object userState)
        {
            base.CancelAsync(userState);
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("wsdl", "2.0.50727.3038")]
    public delegate void ProcessTimeSheetCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);

}
