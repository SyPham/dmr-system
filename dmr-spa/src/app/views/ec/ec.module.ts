// Angular
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { NgxSpinnerModule } from 'ngx-spinner';
// Components Routing
import { ECRoutingModule } from './ec-routing.module';
import { NgSelectModule } from '@ng-select/ng-select';

import { GlueIngredientComponent } from './glue-ingredient/glue-ingredient.component';
import { GlueComponent } from './glue/glue.component';
import { IngredientComponent } from './ingredient/ingredient.component';
import { GlueModalComponent } from './glue/glue-modal/glue-modal.component';
import { IngredientModalComponent } from './ingredient/ingredient-modal/ingredient-modal.component';
import { DropDownListModule } from '@syncfusion/ej2-angular-dropdowns';
import { NgbModalModule, NgbModule } from '@ng-bootstrap/ng-bootstrap';
// Import ngx-barcode module
import { BarcodeGeneratorAllModule, DataMatrixGeneratorAllModule } from '@syncfusion/ej2-angular-barcode-generator';
import { ChartAllModule, AccumulationChartAllModule, RangeNavigatorAllModule } from '@syncfusion/ej2-angular-charts';
import { SwitchModule, RadioButtonModule } from '@syncfusion/ej2-angular-buttons';

import { ModalNameComponent } from './modal-name/modal-name.component';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { ModalNoComponent } from './modal-no/modal-no.component';
import { PlanComponent } from './plan/plan.component';
import { PrintBarCodeComponent } from './print-bar-code/print-bar-code.component';
import { LineComponent } from './line/line.component';
import { SuppilerComponent } from './suppiler/suppiler.component';
import { BuildingComponent } from './building/building.component';
import { BuildingUserComponent } from './building-user/building-user.component';
import { SummaryComponent } from './summary/summary.component';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { AccountComponent } from './account/account.component';
import { BPFCScheduleComponent } from './BPFCSchedule/BPFCSchedule.component';
import { BuildingModalComponent } from './building/building-modal/building-modal.component';
import { QRCodeGeneratorAllModule } from '@syncfusion/ej2-angular-barcode-generator';
import { PrintQRCodeComponent } from './ingredient/print-qrcode/print-qrcode.component';
import { MaskedTextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { HttpClient } from '@angular/common/http';
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
// AoT requires an exported function for factories
export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}
import { PartComponent } from './part/part.component';
import { KindComponent } from './kind/kind.component';
import { MaterialComponent } from './material/material.component';
import { BpfcComponent } from './bpfc/bpfc.component';
import { BpfcStatusComponent } from './bpfc-status/bpfc-status.component';
import { AutofocusDirective } from './focus.directive';
import { AutoSelectDirective } from './select.directive';
import { SearchDirective } from './search.directive';
import { GlueHistoryComponent } from './summary/glue-history/glue-history.component';
import { SelectTextDirective } from './select.text.directive';
import { InventoryComponent } from './inventory/inventory.component';
import { TooltipModule } from '@syncfusion/ej2-angular-popups';
import { DeliveredHistoryComponent } from './delivered-history/delivered-history.component';
import { SearchComponent } from './search/search.component';
import { AbnormalListComponent } from './abnormal-list/abnormal-list.component';
import { StirComponent } from './stir/stir.component';
import { DateTimePickerModule } from '@syncfusion/ej2-angular-calendars';
import { Ng2SearchPipeModule } from 'ng2-search-filter';
import { SelectQrCodeDirective } from './select.qrcode.directive';
import { IncomingComponent } from './incoming/incoming.component';
import { BuildingSettingComponent } from './building-setting/building-setting.component';
import { PlanOutputQuantityComponent } from './plan-output-quantity/plan-output-quantity.component';
import { DatePipe } from '@angular/common';
import { CostingComponent } from './costing/costing.component';
import { ConsumptionComponent } from './consumption/consumption.component';
import { GridAllModule, GridModule } from '@syncfusion/ej2-angular-grids';
import { ScalingSettingComponent } from './scaling-setting/scaling-setting.component';
import { Consumption1Component } from './consumption-1/consumption-1.component';
import { Consumption2Component } from './consumption-2/consumption-2.component';
import { L10n, loadCldr, setCulture, Ajax } from '@syncfusion/ej2-base';
import { TodolistComponent } from './todolist/todolist.component';
import { MixingComponent } from './mixing/mixing.component';
import { DispatchComponent } from './dispatch/dispatch.component';
import { PrintGlueComponent } from './print-glue/print-glue.component';
import { BpfcDetailComponent } from './bpfc-detail/bpfc-detail.component';
import { Bpfc1Component } from './bpfc-1/bpfc-1.component';
import { ToolbarModule } from '@syncfusion/ej2-angular-navigations';
import { AutoSelectDispatchDirective } from './select.dispatch.directive';
import { GlueTypeComponent } from './glue-type/glue-type.component';
import { GlueTypeModalComponent } from './glue-type/glue-type-modal/glue-type-modal.component';
import { TreeGridAllModule } from '@syncfusion/ej2-angular-treegrid/';
import { StationComponent } from './plan/station/station.component';
import { CountdownModule } from 'ngx-countdown';
import { TimePickerModule } from '@progress/kendo-angular-dateinputs';
declare var require: any;
let defaultLang: string;
const lang = localStorage.getItem('lang');
loadCldr(
  require('cldr-data/supplemental/numberingSystems.json'),
  require('cldr-data/main/en/ca-gregorian.json'),
  require('cldr-data/main/en/numbers.json'),
  require('cldr-data/main/en/timeZoneNames.json'),
  require('cldr-data/supplemental/weekdata.json')); // To load the culture based first day of week

loadCldr(
  require('cldr-data/supplemental/numberingSystems.json'),
  require('cldr-data/main/vi/ca-gregorian.json'),
  require('cldr-data/main/vi/numbers.json'),
  require('cldr-data/main/vi/timeZoneNames.json'),
  require('cldr-data/supplemental/weekdata.json')); // To load the culture based first day of week
if (lang === 'vi') {
  defaultLang = lang;
} else {
  defaultLang = 'en';
}
@NgModule({
  providers: [
    DatePipe,
  ],
  imports: [
    ButtonModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    NgxSpinnerModule,
    ECRoutingModule,
    NgSelectModule,
    DropDownListModule,
    NgbModule,
    ChartAllModule,
    AccumulationChartAllModule,
    RangeNavigatorAllModule,
    BarcodeGeneratorAllModule,
    QRCodeGeneratorAllModule,
    DataMatrixGeneratorAllModule,
    SwitchModule,
    MaskedTextBoxModule,
    DatePickerModule,
    TreeGridAllModule,
    GridAllModule,
    RadioButtonModule,
    TooltipModule,
    TimePickerModule,
    Ng2SearchPipeModule,
    DateTimePickerModule,
    CountdownModule,
    ToolbarModule,
    TranslateModule.forChild({
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      },
      defaultLanguage: defaultLang
    }),
  ],
  declarations: [
    GlueIngredientComponent,
    GlueComponent,
    IngredientComponent,
    GlueModalComponent,
    IngredientModalComponent,
    ModalNameComponent,
    ModalNoComponent,
    PlanComponent,
    PrintBarCodeComponent,
    LineComponent,
    SuppilerComponent,
    BuildingComponent,
    BuildingModalComponent,
    BuildingUserComponent,
    AccountComponent,
    BPFCScheduleComponent,
    TodolistComponent,
    SummaryComponent,
    PrintQRCodeComponent,
    PartComponent,
    KindComponent,
    MaterialComponent,
    BpfcComponent,
    ScalingSettingComponent,
    BpfcStatusComponent,
    AutofocusDirective,
    SelectTextDirective,
    AutoSelectDirective,
    AutoSelectDispatchDirective,
    SearchDirective,
    GlueHistoryComponent,
    InventoryComponent,
    DeliveredHistoryComponent,
    SearchComponent,
    AbnormalListComponent,
    StirComponent,
    SelectQrCodeDirective,
    IncomingComponent,
    PlanOutputQuantityComponent,
    CostingComponent,
    BuildingSettingComponent,
    ConsumptionComponent,
    Consumption1Component,
    Consumption2Component,
    MixingComponent,
    DispatchComponent,
    PrintGlueComponent,
    BpfcDetailComponent,
    Bpfc1Component,
    GlueTypeModalComponent,
    GlueTypeComponent,
    StationComponent
  ]
})
export class ECModule {
  vi: any;
  en: any;
  constructor() {
    if (lang === 'vi') {
      defaultLang = 'vi';
      setTimeout(() => {
        L10n.load(require('../../../assets/ej2-lang/vi.json'));
        setCulture('vi');
      });
    } else {
      defaultLang = 'en';
      setTimeout(() => {
        L10n.load(require('../../../assets/ej2-lang/en-US.json'));
        setCulture('en');
      });
    }
  }
 }
