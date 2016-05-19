﻿using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using DevExpress.XtraGrid.Columns;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xafology.ExpressApp.Xpo;
using Xafology.ExpressApp.Xpo.ValueMap;

namespace Xafology.ExpressApp.Paste.Win
{
    public class PasteUtils
    {
        private readonly IXpoFieldValueReader xpoFieldValueReader;

        public PasteUtils(IXpoFieldValueReader xpoFieldReader)
        {
            this.xpoFieldValueReader = xpoFieldReader;
        }

        public PasteUtils()
        {
            xpoFieldValueReader = new XpoFieldValueReader();
        }


        public void PasteColumnsToRow(string[] copiedRowValues, int focusedRowHandle, ListView view)
        {
            var listEditor = ((ListView)view).Editor as GridListEditor;
            var objSpace = ((ListView)view).ObjectSpace;
            var gridView = listEditor.GridView;

            // iterate through columns in listview
            int columnIndex = 0;

            var hasGroups = gridView.Columns.Where(x => x.GroupIndex != -1).Count() > 0;
            if (hasGroups)
                throw new UserFriendlyException("Please ungroup columns before pasting rows");

            var gridColumns = gridView.Columns
                .Where(x => x.Visible && x.GroupIndex == -1)
                .OrderBy(x => x.VisibleIndex)
                .Select(x => x);

            foreach (var gridColumn in gridColumns)
            {
                if (columnIndex >= copiedRowValues.Length)
                    break;

                var member = listEditor.Model.Columns[gridColumn.Name];
                var isLookup = typeof(IXPObject).IsAssignableFrom(member.ModelMember.MemberInfo.MemberType);

                var copiedValue = copiedRowValues[columnIndex];

                // if column is visible in grid, then increment the copiedValue column counter
                if (gridColumn.Visible)
                    columnIndex++;

                // skip non-editable, key column, invisible column or blank values
                // otherwise paste values
                
                var memberInfo = member.ModelMember.MemberInfo;
                if (member.AllowEdit
                    && member.PropertyName != listEditor.Model.ModelClass.KeyProperty
                    && !string.IsNullOrEmpty(copiedValue)
                    && !string.IsNullOrWhiteSpace(copiedValue)
                    )
                {
                    var pasteValue = xpoFieldValueReader.GetMemberValue(((XPObjectSpace)objSpace).Session,
                        memberInfo, copiedValue, true, true);
                    gridView.SetRowCellValue(focusedRowHandle, gridColumn, pasteValue);
                }
            }
        }

        public void PasteColumn(string[] copiedValues, GridListEditor listEditor, IObjectSpace objSpace)
        {
            var gridView = listEditor.GridView;
            var gridColumn = gridView.FocusedColumn;
            var columnKey = gridColumn.FieldName.Replace("!", string.Empty);
            var member = listEditor.Model.ModelClass.OwnMembers[columnKey]; // TODO: remove ! suffice when column is a lookup

            if (!member.AllowEdit)
                throw new InvalidOperationException("Column '" + gridView.FocusedColumn.Caption + "' is not editable.");

            int[] selectedRows = gridView.GetSelectedRows();
            int copyIndex = 0;
            foreach (int rowHandle in selectedRows)
            {
                string copiedValue = copiedValues[copyIndex];

                var pasteValue = xpoFieldValueReader.GetMemberValue(((XPObjectSpace)objSpace).Session,
                    member.MemberInfo, copiedValue, true, true);

                // paste object to focused cell
                gridView.SetRowCellValue(rowHandle, gridView.FocusedColumn, pasteValue);

                if (copyIndex >= copiedValues.Length - 1)
                    // reset copied row counter to the beginning after the last row is reached
                    copyIndex = 0;
                else
                    // go to next copied value
                    copyIndex++;
            }
        }
    }
}
