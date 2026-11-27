# ChampUI for coding agents

Reference for ChampUI Core 0.3.3 and Dashboard Starter 0.1.3 on .NET 10. Start with [application setup](application-setup.md), then choose a component from the inventory. Use the [shared types](types.md), [grid guide](grid.md), [business recipes](recipes/README.md), and [agent guidelines](agent-guidelines.md) when composing features. See the [verification report](audit.md) for scope and reproduction.

## Component inventory

**Actions**

[UiButton](component-reference/UiButton.md), [UiIconButton](component-reference/UiIconButton.md), [UiMenu](component-reference/UiMenu.md)

**Charts**

[ChartText](component-reference/ChartText.md), [UiAreaChart](component-reference/UiAreaChart.md), [UiBarChart](component-reference/UiBarChart.md), [UiDonutChart](component-reference/UiDonutChart.md), [UiLineChart](component-reference/UiLineChart.md)

**Data**

[UiColumn](component-reference/UiColumn.md), [UiDataGrid](component-reference/UiDataGrid.md)

**Display**

[UiAvatar](component-reference/UiAvatar.md), [UiBadge](component-reference/UiBadge.md), [UiEmptyState](component-reference/UiEmptyState.md), [UiIcon](component-reference/UiIcon.md), [UiProgress](component-reference/UiProgress.md), [UiSkeleton](component-reference/UiSkeleton.md), [UiTooltip](component-reference/UiTooltip.md)

**Feedback**

[UiAlert](component-reference/UiAlert.md), [UiDialog](component-reference/UiDialog.md), [UiLoading](component-reference/UiLoading.md), [UiToast](component-reference/UiToast.md), [UiToastHost](component-reference/UiToastHost.md)

**Forms**

[UiCheckbox](component-reference/UiCheckbox.md), [UiDatePicker](component-reference/UiDatePicker.md), [UiFormField](component-reference/UiFormField.md), [UiNumberField](component-reference/UiNumberField.md), [UiSelect](component-reference/UiSelect.md), [UiSwitch](component-reference/UiSwitch.md), [UiTextArea](component-reference/UiTextArea.md), [UiTextField](component-reference/UiTextField.md)

**Layout**

[UiAppShell](component-reference/UiAppShell.md), [UiCard](component-reference/UiCard.md), [UiDivider](component-reference/UiDivider.md), [UiPageHeader](component-reference/UiPageHeader.md), [UiSidebar](component-reference/UiSidebar.md), [UiSidebarItem](component-reference/UiSidebarItem.md), [UiStatCard](component-reference/UiStatCard.md), [UiThemeStyle](component-reference/UiThemeStyle.md), [UiTopbar](component-reference/UiTopbar.md)

**Navigation**

[UiBreadcrumb](component-reference/UiBreadcrumb.md), [UiDropdown](component-reference/UiDropdown.md), [UiPagination](component-reference/UiPagination.md), [UiTab](component-reference/UiTab.md), [UiTabs](component-reference/UiTabs.md)

The inventory includes 43 Ui-prefixed components and the public SVG helper ChartText. Initial values are C# property initializers or CLR defaults. Required means EditorRequired in the source. Components may require a containing parent or scoped services as described in their reference.
