﻿using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using Xafology.ExpressApp.Xpo.Import.Parameters;

namespace Xafology.ExpressApp.Xpo.Import.Controllers
{
    public class ImportParamListViewController : ViewController<ListView>
    {
        public ImportParamListViewController()
        {
            TargetObjectType = typeof(ImportParamBase);
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            var dc = Frame.GetController<DialogController>();
            if (dc != null)
            {
                dc.CanCloseWindow = false;
                dc.Accepting += dc_Accepting;
                dc.Cancelling += dc_Cancelling;
            }
        }

        void dc_Cancelling(object sender, System.EventArgs e)
        {
            View.Close();
        }

        void dc_Accepting(object sender, DialogControllerAcceptingEventArgs e)
        {
            var sourceParam = e.AcceptActionArgs.CurrentObject as Xafology.ExpressApp.Xpo.Import.Parameters.ImportParamBase;
            if (sourceParam == null) return;

            var objSpace = (XPObjectSpace)Application.CreateObjectSpace();
            var targetParam = objSpace.GetObjectByKey(View.CurrentObject.GetType(), objSpace.GetKeyValue(sourceParam));
            var svp = new ShowViewParameters();
            var dc = new DialogController();
            svp.Controllers.Add(dc);
            svp.CreatedView = Application.CreateDetailView(objSpace, targetParam);
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.Context = TemplateContext.PopupWindow;
            Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(Frame, null));
        }
    }
}