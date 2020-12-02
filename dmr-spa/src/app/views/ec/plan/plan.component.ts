import { PlanService } from './../../../_core/_service/plan.service';
import { Plan } from './../../../_core/_model/plan';
import { Component, OnInit, ViewChild, TemplateRef, OnDestroy } from '@angular/core';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { PageSettingsModel, GridComponent} from '@syncfusion/ej2-angular-grids';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';

import { DatePipe } from '@angular/common';

import { FormGroup } from '@angular/forms';
import { BPFCEstablishService } from 'src/app/_core/_service/bpfc-establish.service';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { AuthService } from 'src/app/_core/_service/auth.service';
import { IRole } from 'src/app/_core/_model/role';
import { IBuilding } from 'src/app/_core/_model/building';
import { FilteringEventArgs } from '@syncfusion/ej2-angular-dropdowns';
import { Query } from '@syncfusion/ej2-data/';
import { EmitType } from '@syncfusion/ej2-base';
import { Subscription } from 'rxjs';
import { DataService } from 'src/app/_core/_service/data.service';

const ADMIN = 1;
const SUPERVISER = 2;
@Component({
  selector: 'app-plan',
  templateUrl: './plan.component.html',
  styleUrls: ['./plan.component.css'],
  providers: [
    DatePipe
  ]
})
export class PlanComponent implements OnInit, OnDestroy {
  @ViewChild('cloneModal') public cloneModal: TemplateRef<any>;
  @ViewChild('planForm')
  orderForm: FormGroup;
  pageSettings: PageSettingsModel;
  toolbarOptions: object;
  editSettings: object;
  sortSettings = { columns: [{ field: 'dueDate', direction: 'Ascending' }] };
  startDate: Date;
  endDate: Date;
  date: Date;
  bpfcID: number;
  level: number;
  hasWorker: boolean;
  role: IRole;
  building: IBuilding;
  bpfcData: object;
  plansSelected: any;
  editparams: object;
  @ViewChild('grid')
  grid: GridComponent;
  dueDate: any;
  modalReference: NgbModalRef;
  data: object[];
  searchSettings: any = { hierarchyMode: 'Parent' };
  modalPlan: Plan = {
    id: 0,
    buildingID: 0,
    BPFCEstablishID: 0,
    BPFCName: '',
    hourlyOutput: 0,
    workingHour: 0,
    dueDate: new Date()
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
  subscription: Subscription[] = [];
  constructor(
    private alertify: AlertifyService,
    public modalService: NgbModal,
    private planService: PlanService,
    private buildingService: BuildingService,
    private dataService: DataService,
    private bPFCEstablishService: BPFCEstablishService,
    public datePipe: DatePipe
  ) { }

  ngOnInit(): void {
    this.date = new Date();
    this.endDate = new Date();
    this.startDate = new Date();
    this.hasWorker = false;
    const BUIDLING: IBuilding = JSON.parse(localStorage.getItem('building'));
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    this.role = ROLE;
    this.building = BUIDLING;
    this.gridConfig();
    this.getAll();
    this.getAllBPFC();
    this.checkRole();
    this.ClearForm();
  }
  public onFiltering: EmitType<FilteringEventArgs> = (
    e: FilteringEventArgs
  ) => {
    let query: Query = new Query();
    // frame the query based on search string with filter type.
    query =
      e.text !== '' ? query.where('name', 'contains', e.text, true) : query;
    // pass the filter data source, filter query to updateData method.
    e.updateData(this.BPFCs, query);
  }
  checkRole(): void {
    const ROLES = [ADMIN, SUPERVISER];
    if (ROLES.includes(this.role.id)) {
      this.buildingService.getBuildings().subscribe(async (buildingData) => {
        const lines = buildingData.filter(item => item.level === 3);
        this.buildingName = lines;
      });
    } else {
      this.getAllLine(this.building.id);
    }
  }
  ngOnDestroy() {
    this.subscription.forEach(subscription => subscription.unsubscribe());
  }
  gridConfig(): void {
    this.pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
    this.editparams = { params: { popupHeight: '300px' } };
    this.editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
    this.toolbarOptions = ['ExcelExport', 'Add', 'Update', 'Cancel',
      { text: 'Delete Range', tooltipText: 'Delete Range', prefixIcon: 'fa fa-trash', id: 'DeleteRange' }, 'Search',
      { text: 'Clone', tooltipText: 'Copy', prefixIcon: 'fa fa-copy', id: 'Clone' }
    ];
    const deleteRangeEn = 'Delete Range';
    const cloneEn = 'Clone';
    const deleteRangeVi = 'Xóa Nhiều';
    const cloneVi = 'Nhân Bản';
    this.subscription.push(this.dataService.getValueLocale().subscribe(lang => {
      if (lang === 'vi') {
        this.toolbarOptions = ['Add', 'Update', 'Cancel',
          { text: 'ExcelExport', tooltipText: 'ExcelExport', prefixIcon: 'fa fa-remove', id: 'ExcelExport' },
          { text: deleteRangeVi, tooltipText: deleteRangeVi, prefixIcon: 'fa fa-trash', id: 'DeleteRange' }, 'Search',
          { text: cloneVi, tooltipText: cloneVi, prefixIcon: 'fa fa-copy', id: 'Clone' }
        ];
        return;
      } else if (lang === 'en') {
        this.toolbarOptions = ['Add', 'Update', 'Cancel',
          { text: 'ExcelExport', tooltipText: 'ExcelExport', prefixIcon: 'fa fa-remove', id: 'ExcelExport' },
          { text: deleteRangeEn, tooltipText: deleteRangeEn, prefixIcon: 'fa fa-trash', id: 'DeleteRange' }, 'Search',
          { text: cloneEn, tooltipText: cloneEn, prefixIcon: 'fa fa-copy', id: 'Clone' }
        ];
        return;
      } else {
        const langLocal = localStorage.getItem('lang');
        if (langLocal === 'vi') {
          this.toolbarOptions = ['Add', 'Update', 'Cancel',
            { text: 'ExcelExport', tooltipText: 'ExcelExport', prefixIcon: 'fa fa-remove', id: 'ExcelExport' },
            { text: deleteRangeVi, tooltipText: deleteRangeVi, prefixIcon: 'fa fa-trash', id: 'DeleteRange' }, 'Search',
            { text: cloneVi, tooltipText: cloneVi, prefixIcon: 'fa fa-copy', id: 'Clone' }
          ];
          return;
        } else if (langLocal === 'en') {
          this.toolbarOptions = ['Add', 'Update', 'Cancel',
            { text: 'ExcelExport', tooltipText: 'ExcelExport', prefixIcon: 'fa fa-remove', id: 'ExcelExport' },
            { text: deleteRangeEn, tooltipText: deleteRangeEn, prefixIcon: 'fa fa-trash', id: 'DeleteRange' }, 'Search',
            { text: cloneEn, tooltipText: cloneEn, prefixIcon: 'fa fa-copy', id: 'Clone' }
          ];
          return;
        }
      }
    }));
  }
  count(index) {
    return Number(index) + 1;
  }
  onDoubleClick(args: any): void {
    this.setFocus = args.column; // Get the column from Double click event
  }
  getAllLine(buildingID) {
    this.planService.getLines(buildingID).subscribe((res: any) => {
      this.buildingName = res;
    });
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

  actionComplete(args) {
    if (args.requestType === 'beginEdit') {
      if (this.setFocus.field !== 'buildingName' && this.setFocus.field !== 'bpfcName') {
        (args.form.elements.namedItem(this.setFocus?.field) as HTMLInputElement).focus();
      }
    }
  }
  actionBegin(args) {
    if (args.requestType === 'cancel') {
      this.ClearForm();
    }

    if (args.requestType === 'save') {
      if (args.action === 'edit') {
        this.modalPlan.id = args.data.id || 0;
        this.modalPlan.buildingID = this.buildingNameEdit;
        this.modalPlan.dueDate = this.dueDate;
        this.modalPlan.workingHour = args.data.workingHour;
        this.modalPlan.BPFCEstablishID = args.data.bpfcEstablishID;
        this.modalPlan.BPFCName = args.data.bpfcName;
        this.modalPlan.hourlyOutput = args.data.hourlyOutput;
        this.planService.update(this.modalPlan).subscribe(res => {
          this.alertify.success('Cập nhật thành công! <br>Updated succeeded!');
          this.ClearForm();
          this.getAll();
        });
      }
      if (args.action === 'add') {
        this.modalPlan.buildingID = this.buildingNameEdit;
        this.modalPlan.dueDate = this.dueDate;
        this.modalPlan.workingHour = args.data.workingHour || 0;
        this.modalPlan.BPFCEstablishID = this.bpfcEdit;
        this.modalPlan.BPFCName = args.data.bpfcName;
        this.modalPlan.hourlyOutput = args.data.hourlyOutput || 0;
        this.planService.create(this.modalPlan).subscribe(res => {
          if (res) {
            this.alertify.success('Tạo thành công!<br>Created succeeded!');
            this.getAll();
            this.ClearForm();
          } else {
            this.alertify.warning('Dữ liệu đã tồn tại! <br>This plan has already existed!!!');
            this.getAll();
            this.ClearForm();
          }
        }, error => {
            this.grid.refresh();
            this.getAll();
            this.ClearForm();
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

  getAll() {
    this.planService.search(this.building.id, this.startDate.toDateString(), this.endDate.toDateString()).subscribe((res: any) => {
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
          glues: item.glues || []
        };
      });
    });
  }
  deleteRange(plans) {
    this.alertify.confirm('Delete Plan <br> Xóa kế hoạc làm việc', 'Are you sure you want to delete this Plans ?<br> Bạn có chắc chắn muốn xóa không?', () => {
      this.planService.deleteRange(plans).subscribe(() => {
        this.getAll();
        this.alertify.success('Xóa thành công! <br>Plans has been deleted');
      }, error => {
        this.alertify.error('Xóa thất bại! <br>Failed to delete the Model Name');
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
      this.alertify.warning('Hãy chọn 1 hoặc nhiều dòng để nhân bản!<br>Please select the plan!', true);
    }
  }

  toolbarClick(args: any): void {
    switch (args.item.id) {
      case 'Clone':
        this.openModal(this.cloneModal);
        break;
      case 'DeleteRange':
        if (this.grid.getSelectedRecords().length === 0) {
          this.alertify.warning('Hãy chọn 1 hoặc nhiều dòng để xóa <br>Please select the plans!!', true);
        } else {
          const selectedRecords = this.grid.getSelectedRecords().map((item: any) => {
            return item.id;
          });
          this.deleteRange(selectedRecords);
        }
        break;
      case 'ExcelExport':
        this.grid.excelExport();
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
        this.alertify.success('Nhân bản thành công! <br>Successfully!');
        this.startDate = this.date;
        this.endDate = this.date;
        this.getAll();
        this.modalService.dismissAll();
      } else {
        this.alertify.warning('Dữ liệu này đã tồn tại!<br>The plans have already existed!');
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
          glues: item.glues || []
        };
      });
    });
  }


  onClickDefault() {
    this.startDate = new Date();
    this.endDate = new Date();
    this.getAll();
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
      const array = ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10'];
      const glues = [];
      for (const item of data) {
        if (!array.includes(item)) {
          glues.push(item);
        }
      }
      return glues.join('<br>');
    } else {
      return '';
    }
  }

  onClickFilter() {
    this.search(this.startDate, this.endDate);
  }

  // End API
}
