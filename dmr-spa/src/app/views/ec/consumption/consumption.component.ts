import { PlanService } from './../../../_core/_service/plan.service';
import { Plan } from './../../../_core/_model/plan';
import { Component, OnInit, ViewChild, TemplateRef } from '@angular/core';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { PageSettingsModel, GridComponent, CellEditArgs } from '@syncfusion/ej2-angular-grids';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { ActivatedRoute } from '@angular/router';
import { DatePipe } from '@angular/common';
import { FormGroup } from '@angular/forms';
import { BPFCEstablishService } from 'src/app/_core/_service/bpfc-establish.service';
import { NgxSpinnerService } from 'ngx-spinner';
const WORKER = 4;
const BUILDING_LEVEL = 2;

@Component({
  selector: 'app-consumption',
  templateUrl: './consumption.component.html',
  styleUrls: ['./consumption.component.css']
})
export class ConsumptionComponent implements OnInit {
  @ViewChild('cloneModal') public cloneModal: TemplateRef<any>;
  @ViewChild('planForm')
  public orderForm: FormGroup;
  public pageSettings: PageSettingsModel;
  public toolbarOptions: object;
  public editSettings: object;
  startDate = new Date();
  endDate = new Date();
  bpfcID: number;
  level: number;
  hasWorker: boolean;
  sortSettings = { columns: [{ field: 'dueDate', direction: 'Ascending' }] };
  public bpfcData: object;
  public plansSelected: any;
  public date = new Date();
  public toolbar: string[];
  public editparams: object;
  public role = JSON.parse(localStorage.getItem('level'));
  public building = JSON.parse(localStorage.getItem('building'));
  @ViewChild('grid')
  public grid: GridComponent;
  dueDate: any;
  modalReference: NgbModalRef;
  public data: object[];
  searchSettings: any = { hierarchyMode: 'Parent' };
  modalPlan: Plan = {
    id: 0,
    buildingID: 0,
    BPFCEstablishID: 0,
    BPFCName: '',
    hourlyOutput: 0,
    workingHour: 0,
    dueDate: new Date(),
    startWorkingTime: new Date(),
    finishWorkingTime: new Date(),
    startTime: {
      hour: 7,
      minute: 0
    },
    endTime: {
      hour: 16,
      minute: 30
    }

  };
  public textLine = 'Select a line name';
  public fieldsGlue: object = { text: 'name', value: 'name' };
  public fieldsLine: object = { text: 'name', value: 'name' };
  public fieldsBPFC: object = { text: 'name', value: 'name' };
  public buildingName: object[];
  public modelName: object[];
  buildingNameEdit: any;
  workHour: number;
  hourlyOutput: number;
  BPFCs: any;
  bpfcEdit: number;
  glueDetails: any;
  setFocus: any;
  constructor(
    private route: ActivatedRoute,
    private alertify: AlertifyService,
    public modalService: NgbModal,
    private planService: PlanService,
    private bPFCEstablishService: BPFCEstablishService,
    public datePipe: DatePipe,
    private spinner: NgxSpinnerService
  ) { }

  ngOnInit(): void {
    const now = new Date();
    this.endDate = new Date();
    this.level = JSON.parse(localStorage.getItem('level')).level;
    this.pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
    this.editparams = { params: { popupHeight: '300px' } };
    if (this.level === WORKER) {
      this.hasWorker = true;
      this.editSettings = { showDeleteConfirmDialog: false, allowEditing: false, allowAdding: false, allowDeleting: false, mode: 'Normal' };
      this.toolbarOptions = ['ExcelExport', 'Search'];
    } else {
      this.hasWorker = false;
      this.editSettings = { showDeleteConfirmDialog: false, allowEditing: false, allowAdding: false, allowDeleting: false, mode: 'Normal' };
      this.toolbarOptions = ['ExcelExport', 'Search'];
    }
    this.toolbar = ['ExcelExport', 'Search'];
    this.getAll(this.startDate, this.endDate);
    this.getAllBPFC();
    const buildingID = JSON.parse(localStorage.getItem('level')).id;
    this.getAllLine(buildingID);
    this.ClearForm();
  }
  count(index) {
    return Number(index) + 1;
  }

  getAllLine(buildingID) {
    this.planService.getLines(buildingID).subscribe((res: any) => {
      this.buildingName = res;
    });
  }
  getReport(obj: { startDate: Date, endDate: Date }) {
    const days = Math.floor((obj.endDate.getTime() - obj.startDate.getTime()) / (1000 * 60 * 60 * 24));
    if (days > 31) {
      const error = 'Chỉ được xuất dữ liệu báo cáo trong 30 ngày!!!<br>The report data can only be exported for 30 days!!!';
      this.alertify.error(error, true);
      return;
    }
    this.spinner.show();
    this.planService.getReport(obj).subscribe((data: any) => {
      const blob = new Blob([data],
        { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

      const downloadURL = window.URL.createObjectURL(data);
      const link = document.createElement('a');
      link.href = downloadURL;
      link.download = 'report.xlsx';
      link.click();
      this.spinner.hide();
    });
  }
  // Method is use to download file.
  // param data - Array Buffer data
  // param type - type of the document.
  downLoadFile(data: any, type: string) {
    const blob = new Blob([data], { type });
    const url = window.URL.createObjectURL(blob);
    const pwa = window.open(url);
    if (!pwa || pwa.closed || typeof pwa.closed === 'undefined') {
      this.alertify.error('Please disable your Pop-up blocker and try again.');
    }
    this.spinner.hide();
  }
  onChangeBuildingNameEdit(args) {
    this.buildingNameEdit = args.itemData.id;
  }
  onChangeDueDateEdit(args) {
    this.dueDate = (args.value as Date).toDateString();
  }

  onChangeDueDateClone(args) {
    this.date = (args.value as Date);
  }

  onChangeBPFCEdit(args) {
    this.bpfcEdit = args.itemData.id;
  }

  actionComplete(e) {
    if (e.requestType === 'beginEdit') {
      e.form.elements.namedItem(this.setFocus.field).focus(); // Set focus to the Target element
    }
  }
  onDoubleClick(args: any): void {
    this.setFocus = args.column; // Get the column from Double click event
  }
  actionBegin(args) {
    if (args.requestType === 'cancel') {
      this.ClearForm();
    }

    if (args.requestType === 'save') {
      if (args.action === 'edit') {
        const planId = args.data.id || 0;
        const quantity = args.data.quantity;
        this.planService.editQuantity(planId, quantity).subscribe(res => {
          this.alertify.success('Updated succeeded!');
          this.ClearForm();
          this.getAll(this.startDate, this.endDate);
        });
      }
    }
  }

  private ClearForm() {
    this.bpfcEdit = 0;
    this.hourlyOutput = 0;
    this.workHour = 0;
    this.dueDate = new Date();
  }

  private validForm(): boolean {
    const array = [this.bpfcEdit];
    return array.every(item => item > 0);
  }

  onChangeWorkingHour(args) {
    this.workHour = args;
  }

  onChangeHourlyOutput(args) {

    this.hourlyOutput = args;
  }

  rowSelected(args) {
  }

  openaddModalPlan(addModalPlan) {
    this.modalReference = this.modalService.open(addModalPlan);
  }

  getAllBPFC() {
    this.bPFCEstablishService.filterByApprovedStatus().subscribe((res: any) => {
      this.BPFCs = res.map((item) => {
        return {
          id: item.id,
          name: `${item.modelName} - ${item.modelNo} - ${item.articleNo} - ${item.artProcess}`,
        };
      });
    });
  }

  getAll(startDate, endDate) {
    this.planService.search(this.building.id, startDate.toDateString(), endDate.toDateString()).subscribe((res: any) => {
      this.data = res.map(item => {
        return {
          id: item.id,
          bpfcName: `${item.modelName} - ${item.modelNoName} - ${item.articleName} - ${item.processName}`,
          dueDate: item.dueDate,
          createdDate: item.createdDate,
          workingHour: item.workingHour,
          hourlyOutput: item.hourlyOutput,
          buildingName: item.buildingName,
          buildingID: item.buildingID,
          quantity: item.quantity,
          bpfcEstablishID: item.bpfcEstablishID,
          glues: item.glues || []
        };
      });
    });
  }
  deleteRange(plans) {
    this.alertify.confirm('Delete Plan', 'Are you sure you want to delete this Plans ?', () => {
      this.planService.deleteRange(plans).subscribe(() => {
        this.getAll(this.startDate, this.endDate);
        this.alertify.success('Plans has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the Modal Name');
      });
    });
  }

  /// Begin API
  openModal(ref) {
    const selectedRecords = this.grid.getSelectedRecords();
    if (selectedRecords.length !== 0) {
      this.plansSelected = selectedRecords.map((item: any) => {
        return {
          id: 0,
          bpfcEstablishID: item.bpfcEstablishID,
          workingHour: item.workingHour,
          hourlyOutput: item.hourlyOutput,
          dueDate: item.dueDate,
          buildingID: item.buildingID
        };
      });
      this.modalReference = this.modalService.open(ref);
    } else {
      this.alertify.warning('Please select the plan');
    }
  }

  toolbarClick(args: any): void {
    if (args.item.id.indexOf('excelexport') !== -1) {
         this.getReport({ startDate: this.startDate, endDate: this.endDate });
    }
    switch (args.item.id) {
      case 'Clone':
        this.openModal(this.cloneModal);
        break;
      case 'Delete Range':
        if (this.grid.getSelectedRecords().length === 0) {
          this.alertify.warning('Please select the plans!!');
        } else {
          const selectedRecords = this.grid.getSelectedRecords().map((item: any) => {
            return item.id;
          });
          this.deleteRange(selectedRecords);
        }
        break;
      case 'Excel Export':
        this.getReport({ startDate: this.startDate, endDate: this.endDate });
        break;
      default:
        break;
    }
  }

  onClickClone() {
    this.plansSelected.map(item => {
      item.dueDate = this.date;
    });

    this.planService.clonePlan(this.plansSelected).subscribe((res: any) => {
      if (res) {
        this.alertify.success('Successfully!');
        this.startDate = this.date;
        this.endDate = this.date;
        this.getAll(this.date, this.date);
        this.modalService.dismissAll();
      } else {
        this.alertify.warning('the plans have already existed!');
        this.modalService.dismissAll();
      }
    });
  }

  search(startDate, endDate) {
    this.planService.search(this.building.id, startDate.toDateString(), endDate.toDateString()).subscribe((res: any) => {
      this.data = res.map(item => {
        return {
          id: item.id,
          bpfcName: `${item.modelName} - ${item.modelNoName} - ${item.articleName} - ${item.processName}`,
          dueDate: item.dueDate,
          createdDate: item.createdDate,
          workingHour: item.workingHour,
          hourlyOutput: item.hourlyOutput,
          buildingName: item.buildingName,
          buildingID: item.buildingID,
          bpfcEstablishID: item.bpfcEstablishID,
          quantity: item.quantity,
          glues: item.glues || [],
        };
      });
    });
  }


  onClickDefault() {
    this.startDate = new Date();
    this.endDate = new Date();
    this.getAll(this.startDate, this.endDate);
  }
  startDateOnchange(args) {
    this.startDate = (args.value as Date);
    this.search(this.startDate, this.endDate);
  }
  endDateOnchange(args) {
    this.endDate = (args.value as Date);
    this.search(this.startDate, this.endDate);
  }
  tooltip(data) {
    if (data) {
      return data.join('<br>');
    } else {
      return '';
    }
  }
  editQuantity(id: number, qty: number) {
    this.planService.editQuantity(id, qty);
  }
  onClickFilter() {
    this.search(this.startDate, this.endDate);
  }

  // End API
}
