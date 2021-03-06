﻿using Xafology.ExpressApp.PivotGridLayout.Helpers;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using System;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Editors;

namespace Xafology.ExpressApp.PivotGridLayout.Controllers
{
    public delegate void PivotGridLayoutEventHandler(object sender);
    public delegate void PivotGridLayoutFieldsMappedEventHandler(object sender, PivotGridLayoutEventArgs e);

    public class PivotGridLayoutController : ViewController
    {
        public event PivotGridLayoutEventHandler PivotGridLayoutReset;

        public PivotGridSetup PivotGridSetupObject;
        public readonly string DefaultLayoutActionCaption = "Layout";
        private readonly SingleChoiceAction _LayoutAction = null;
        private const string saveChoiceCaption = "Save";
        private const string loadChoiceCaption = "Load";
        private const string resetLayoutChoiceCaption = "Reset";

        public PivotGridLayoutController()
        {
            TargetViewType = ViewType.ListView;

            // unique view ID so that it will not match any existing views
            // unless the developer assigns it another View ID in a derived constructor
            TargetViewId = "{44F8DE78-5DBB-4316-AFDC-8F8A58D4E2FC}";
            
            // TODO: use this.Active instead

            _LayoutAction = new SingleChoiceAction(this, "LayoutAction", DevExpress.Persistent.Base.PredefinedCategory.View);
            _LayoutAction.Caption = DefaultLayoutActionCaption;
            _LayoutAction.ItemType = SingleChoiceActionItemType.ItemIsOperation;
            _LayoutAction.ShowItemsOnClick = true;
            _LayoutAction.Execute += myLayoutAction_Execute;

            var resetLayoutChoice = new ChoiceActionItem();
            resetLayoutChoice.Caption = resetLayoutChoiceCaption;
            _LayoutAction.Items.Add(resetLayoutChoice);

            var saveChoice = new ChoiceActionItem();
            saveChoice.Caption = saveChoiceCaption;
            _LayoutAction.Items.Add(saveChoice);

            var loadChoice = new ChoiceActionItem();
            loadChoice.Caption = loadChoiceCaption;
            _LayoutAction.Items.Add(loadChoice);

        }

        public SingleChoiceAction LayoutAction
        {
            get
            {
                return _LayoutAction;
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
        }

        protected void UpdateLayoutActionCaption(PivotGridSavedLayout savedLayoutObj)
        {
            //LayoutAction.Caption = String.Format("{0}: {1}", DefaultLayoutActionCaption, savedLayoutObj.LayoutName);
            View.Caption = View.ObjectTypeInfo.Name + " - " + savedLayoutObj.LayoutName;
        }

        /// <summary>
        /// used by ASP.NET to cache the PivotGrid XML stream
        /// and only load the stream to the control when the control is refreshed.
        /// </summary>
        protected virtual void CacheLayoutStream()
        {
        }

        private void myLayoutAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
        {
            switch (e.SelectedChoiceActionItem.Caption)
            {
                case resetLayoutChoiceCaption:
                    ResetPivotGridLayout();
                    break;
                case saveChoiceCaption:
                    {
                        CacheLayoutStream();
                        var controller = new SaveLayoutPopupListViewController();
                        CreateLayoutListView(e.ShowViewParameters, Data.PivotGridSavedLayoutUISaveListViewId, controller);
                    }
                    break;
                case loadChoiceCaption:
                    { 
                        CacheLayoutStream();
                        var controller = new LoadLayoutPopupListViewController();
                        CreateLayoutListView(e.ShowViewParameters, Data.PivotGridSavedLayoutUILoadListViewId, controller);
                    }
                    break;
            }
        }

        private void CreateLayoutListView(ShowViewParameters svp, string listViewId, LayoutPopupListViewController controller)
        {
            IObjectSpace objectSpace = Application.CreateObjectSpace();

            var collectionSource = new CollectionSource(objectSpace, typeof(PivotGridSavedLayout));
            collectionSource.Criteria["UIFilter"] = SavedLayoutUICriteria;

            var listView = Application.CreateListView(
                listViewId,
                collectionSource,
                true);

            svp.TargetWindow = TargetWindow.NewModalWindow;

            
            controller.PivotGridLayoutController = this;
            svp.Controllers.Add(controller);

            svp.CreatedView = listView;
        }
        
        protected virtual void ResetPivotGridLayouts(PivotGridSetup pivotSetup)
        {
        }
        
        public void SaveLayout(PivotGridSavedLayout savedLayoutObj)
        {
            if (savedLayoutObj != null)
                SavePivotGridLayout(savedLayoutObj);
        }

        public void LoadLayout(PivotGridSavedLayout savedLayoutObj)
        {
            if (savedLayoutObj != null)
                LoadPivotGridLayout(savedLayoutObj);
        }

        protected virtual void ResetPivotGridLayout()
        {
            if (PivotGridSetupObject == null)
                throw new ArgumentNullException("pivotSetup");

            LayoutAction.Caption = DefaultLayoutActionCaption;
            ResetPivotGridLayouts(PivotGridSetupObject);

            if (PivotGridLayoutReset != null)
                PivotGridLayoutReset(this);
        }

        protected virtual void SavePivotGridLayout(PivotGridSavedLayout layoutObj)
        {
        }

        protected virtual void LoadPivotGridLayout(PivotGridSavedLayout layoutObj)
        {
        }

        public static PivotGridLoadedLayout FindLoadedLayout(IObjectSpace os, UIPlatform platform, string typeName)
        {
            return FindLoadedLayout(((XPObjectSpace)os).Session, platform, typeName);
        }

        public static PivotGridLoadedLayout FindLoadedLayout(Session session, UIPlatform platform, string typeName)
        {
            var currentUser = StaticHelpers.GetCurrentUser(session);
            CriteriaOperator criteria = null;
            if (currentUser == null)
                criteria = CriteriaOperator.Parse(
                 "UIPlatform = ? And User Is Null And TypeName = ?", platform, typeName);
            else
                criteria = CriteriaOperator.Parse(
                 "UIPlatform = ? And User = ? And TypeName = ?", platform, currentUser, typeName);
            var loadedLayoutObj = session.FindObject<PivotGridLoadedLayout>(criteria);
            return loadedLayoutObj;
        }

        public virtual CriteriaOperator SavedLayoutUICriteria
        {
            get
            {
                return null;
            }
        }
    }
}
