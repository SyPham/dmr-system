import { PlanService } from './../../../_core/_service/plan.service';
import { IPlan, ITime, Plan } from './../../../_core/_model/plan';
import { Component, OnInit, ViewChild, TemplateRef, OnDestroy, AfterViewInit } from '@angular/core';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { PageSettingsModel, GridComponent, SelectionService, QueryCellInfoEventArgs } from '@syncfusion/ej2-angular-grids';
import { NgbModal, NgbModalRef, NgbTimepicker } from '@ng-bootstrap/ng-bootstrap';
import { DatePipe } from '@angular/common';
import { FormGroup } from '@angular/forms';
import { BPFCEstablishService } from 'src/app/_core/_service/bpfc-establish.service';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { IRole } from 'src/app/_core/_model/role';
import { IBuilding } from 'src/app/_core/_model/building';
import { FilteringEventArgs } from '@syncfusion/ej2-angular-dropdowns';
import { Query } from '@syncfusion/ej2-data/';
import { EmitType } from '@syncfusion/ej2-base';
import { Subscription } from 'rxjs';
import { DataService } from 'src/app/_core/_service/data.service';
import { TodolistService } from 'src/app/_core/_service/todolist.service';
import { Tooltip } from '@syncfusion/ej2-angular-popups';
import { StationComponent } from './station/station.component';
import { StationService } from 'src/app/_core/_service/station.service';
declare var $;
const ADMIN = 1;
const BUILDING_LEVEL = 2;
const SUPERVISOR = 2;
@Component({
  selector: 'app-plan',
  templateUrl: './plan.component.html',
  styleUrls: ['./plan.component.css'],
  providers: [
    DatePipe,
    SelectionService
  ]
})
export class PlanComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('cloneModal') public cloneModal: TemplateRef<any>;
  @ViewChild('stationModal') public stationModal: TemplateRef<any>;
  @ViewChild('planForm')
  orderForm: FormGroup;
  @ViewChild('timepicker') startTimepicker: NgbTimepicker;
  startTime: ITime = { hour: 7, minute: 0 };
  endTime: ITime = { hour: 16, minute: 30 };
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
  data: IPlan[];
  plan: IPlan;
  searchSettings: any = { hierarchyMode: 'Parent' };
  modalPlan: Plan = {
    id: 0,
    buildingID: 0,
    BPFCEstablishID: 0,
    BPFCName: '',
    hourlyOutput: 0,
    workingHour: 0,
    dueDate: new Date(),
    startWorkingTime: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDay(), 7, 0, 0),
    finishWorkingTime: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDay(), 16, 30, 0),
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
  fieldsBuildings: object = { text: 'name', value: 'id' };
  startWorkingTimeParams = { params: { value: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate(), 7, 30, 0) } };
  // tslint:disable-next-line:max-line-length
  finishWorkingTimeParams = { params: { value: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate(), 16, 30, 0) } };
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
  selectOptions: object;
  hourlyOutputRules: object;
  numericParams: object;
  buildings: IBuilding[];
  IsAdmin: boolean;
  buildingID: number;
  constructor(
    private alertify: AlertifyService,
    public modalService: NgbModal,
    private planService: PlanService,
    private todolistService: TodolistService,
    private buildingService: BuildingService,
    private stationSevice: StationService,
    private dataService: DataService,
    private bPFCEstablishService: BPFCEstablishService,
    public datePipe: DatePipe
  ) { }
  ngAfterViewInit(): void {
  }

  ngOnInit(): void {
    this.buildingID = 0;
    this.numericParams = { params: { value: 120 }, format: '####' };
    this.hourlyOutputRules = {
      required: true
    };
    this.date = new Date();
    this.endDate = new Date();
    this.startDate = new Date();
    this.hasWorker = false;
    const BUIDLING: IBuilding = JSON.parse(localStorage.getItem('building'));
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    this.role = ROLE;
    this.building = BUIDLING;
    this.watch();
    this.gridConfig();
    this.getAllBPFC();
    this.checkRole();
    this.clearForm();
  }
  onTimeChange(agrs) {
    console.log(agrs);
    // this.endTime = {hour: +value.hour, minute: +value.minute};
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
  watch() {
    const watchAction = this.stationSevice.getValue().subscribe(status => {
      if (status === true) {
        this.getAll();
      }
    });
    this.subscription.push(watchAction);
  }
  checkRole(): void {
    const roles = [ADMIN, SUPERVISOR];
    if (roles.includes(this.role.id)) {
      this.IsAdmin = true;
      const buildingId = +localStorage.getItem('buildingID');
      if (buildingId === 0) {
        this.alertify.message('Please select a building!', true);
        this.getBuilding(() => { });
      } else {
        this.getBuilding(() => {
          this.buildingID = buildingId;
          this.getAll();
        });
      }
    } else {
      this.getBuilding(() => {
        this.buildingID = this.building.id;
        this.getAll();
      });
    }
  }
  ngOnDestroy() {
    this.subscription.forEach(subscription => subscription.unsubscribe());
  }
  onFilteringBuilding: EmitType<FilteringEventArgs> = (
    e: FilteringEventArgs
  ) => {
    let query: Query = new Query();
    // frame the query based on search string with filter type.
    query =
      e.text !== '' ? query.where('name', 'contains', e.text, true) : query;
    // pass the filter data source, filter query to updateData method.
    e.updateData(this.buildings as any, query);
  }
  onChangeBuilding(args) {
    this.buildingID = args.itemData.id;
  }
  onSelectBuilding(args: any): void {
    this.buildingID = args.itemData.id;
    this.buildingName = args.itemData.name;
    localStorage.setItem('buildingID', args.itemData.id);
    this.getAll();
  }
  gridConfig(): void {
    this.selectOptions = { checkboxOnly: true };
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
        this.toolbarOptions = ['Add',
          { text: 'Cập Nhật', tooltipText: 'Cập Nhật', prefixIcon: 'fa fa-tasks', id: 'Update' },
          { text: 'Nhân Bản', tooltipText: 'Nhân Bản', prefixIcon: 'fa fa-clone', id: 'Clone' },
        ];
        return;
      } else if (lang === 'en') {
        this.toolbarOptions = ['Add',
          { text: 'Update', tooltipText: 'Update', prefixIcon: 'fa fa-tasks', id: 'Update' },
          { text: 'Clone', tooltipText: 'Clone', prefixIcon: 'fa fa-clone', id: 'Clone' },
        ];
        return;
      } else {
        const langLocal = localStorage.getItem('lang');
        if (langLocal === 'vi') {
          this.toolbarOptions = ['Add',
            { text: 'Cập Nhật', tooltipText: 'Cập Nhật', prefixIcon: 'fa fa-tasks', id: 'Update' },
            { text: 'Nhân Bản', tooltipText: 'Nhân Bản', prefixIcon: 'fa fa-clone', id: 'Clone' },
          ];
          return;
        } else if (langLocal === 'en') {
          this.toolbarOptions = ['Add',
            { text: 'Update', tooltipText: 'Update', prefixIcon: 'fa fa-tasks', id: 'Update' },
            { text: 'Clone', tooltipText: 'Clone', prefixIcon: 'fa fa-clone', id: 'Clone' },
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
  createdLine() {
    this.buildingID = this.buildingID === undefined ? 0 : this.buildingID;
    this.getAllLine(this.buildingID);
  }
  getBuilding(callback): void {
    this.buildingService.getBuildings().subscribe(async (buildingData) => {
      this.buildings = buildingData.filter(item => item.level === BUILDING_LEVEL);
      callback();
    });
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
    this.dueDate = (args.value as Date)?.toDateString();
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
        // (args.form.elements.namedItem(this.setFocus?.field) as HTMLInputElement).focus();
      }
    }
  }
  convertTimeToDatetime(time: { hour: number, minute: number }) {
    const date = new Date().toDateString() + ` ${time.hour}:${time.minute}`;
    return date;
  }
  convertDatetimeToTime(date: Date) {
    const value = date;
    const time = { hour: value.getHours(), minute: value.getMinutes() };
    return time;
  }
  rowDataBound(args) {
    // khong xai nua
    // if (args.data.isGenerate === true) {
    //   args.row.getElementsByClassName('e-gridchkbox')[0].classList.add('disablecheckbox');
    //   args.row.getElementsByClassName('e-checkbox-wrapper')[0].classList.add('disablecheckbox');
    // }
  }
  onChangeStartTime(value: Date) {
    // this.startTime = { hour: value.getHours(), minute: value.getMinutes() };
    // this.modalPlan.startTime = { hour: value.getHours(), minute: value.getMinutes() };

  }
  onChangeEndTime(value: Date) {
    // this.endTime = { hour: value.getHours(), minute: value.getMinutes() };
    // this.modalPlan.endTime = { hour: value.getHours(), minute: value.getMinutes() };
  }
  actionBegin(args) {
    console.log(this.data);
    if (args.requestType === 'beginEdit') {
      this.clearForm();
      const data = args.rowData;
      this.modalPlan.finishWorkingTime = data.finishWorkingTime;
      this.modalPlan.startWorkingTime = data.startWorkingTime;
      this.modalPlan.startTime =
      {
        hour: this.modalPlan.startWorkingTime.getHours(),
        minute: this.modalPlan.startWorkingTime.getMinutes()
      };
      this.modalPlan.endTime =
      {
        hour: this.modalPlan.finishWorkingTime.getHours(),
        minute: this.modalPlan.finishWorkingTime.getMinutes()
      };
    }
    if (args.requestType === 'cancel') {
      this.clearForm();
      this.grid.refresh();
    }

    if (args.requestType === 'save') {
      if (args.action === 'edit') {
        this.modalPlan.id = args.data.id || 0;
        this.modalPlan.buildingID = this.buildingNameEdit ?? args.data.buildingID;
        this.modalPlan.dueDate = this.dueDate;
        this.modalPlan.workingHour = args.data.workingHour;
        this.modalPlan.BPFCEstablishID = args.data.bpfcEstablishID;
        this.modalPlan.BPFCName = args.data.bpfcName;
        this.modalPlan.hourlyOutput = args.data.hourlyOutput;
        this.modalPlan.startTime =
        {
          hour: this.modalPlan.startWorkingTime.getHours(),
          minute: this.modalPlan.startWorkingTime.getMinutes()
        };
        this.modalPlan.endTime =
        {
          hour: this.modalPlan.finishWorkingTime.getHours(),
          minute: this.modalPlan.finishWorkingTime.getMinutes()
        };
        // this.modalPlan.finishWorkingTime = args.data.finishWorkingTime;
        // this.modalPlan.startWorkingTime = args.data.startWorkingTime;

        this.planService.update(this.modalPlan).subscribe(res => {
          this.alertify.success('Cập nhật thành công! <br>Updated succeeded!');
          this.clearForm();
          this.getAll();
        }, error => {
          this.alertify.error(error, true);
          this.grid.refresh();
          this.getAll();
          this.clearForm();
        });
      }
      if (args.action === 'add') {
        args.data.hourlyOutput = 120;
        this.modalPlan.buildingID = this.buildingNameEdit;
        this.modalPlan.dueDate = this.dueDate;
        this.modalPlan.workingHour = args.data.workingHour || 0;
        this.modalPlan.BPFCEstablishID = this.bpfcEdit;
        this.modalPlan.BPFCName = args.data.bpfcName;
        this.modalPlan.hourlyOutput = args.data.hourlyOutput || 0;
        this.modalPlan.startTime =
        {
          hour: this.modalPlan.startWorkingTime.getHours(),
          minute: this.modalPlan.startWorkingTime.getMinutes()
        };
        this.modalPlan.endTime =
        {
          hour: this.modalPlan.finishWorkingTime.getHours(),
          minute: this.modalPlan.finishWorkingTime.getMinutes()
        };
        this.planService.create(this.modalPlan).subscribe(res => {
          if (res) {
            this.alertify.success('Tạo thành công!<br>Created succeeded!');
            this.getAll();
            this.clearForm();
          } else {
            this.alertify.warning('Dữ liệu đã tồn tại! <br>This plan has already existed!!!');
            this.getAll();
            this.clearForm();
          }
        }, error => {
          this.alertify.error(error, true);
          this.grid.refresh();
          this.getAll();
          this.clearForm();
        });
      }
    }
  }

  private clearForm() {
    this.modalPlan = {
      id: 0,
      buildingID: 0,
      BPFCEstablishID: 0,
      BPFCName: '',
      hourlyOutput: 0,
      workingHour: 0,
      dueDate: new Date(),
      startWorkingTime: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDay(), 7, 0, 0),
      finishWorkingTime: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDay(), 16, 30, 0),
      startTime: {
        hour: 7,
        minute: 0
      },
      endTime: {
        hour: 16,
        minute: 30
      }

    };
    this.startTime = {
      hour: 7,
      minute: 0
    };
    this.endTime = {
      hour: 16,
      minute: 30
    };
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
    this.planService.search(this.buildingID, this.startDate.toDateString(), this.endDate.toDateString()).subscribe((res: any) => {
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
          startWorkingTime: new Date(item.startWorkingTime),
          finishWorkingTime: new Date(item.finishWorkingTime),
          startTime: item.startTime,
          endTime: item.endTime,
          bpfcEstablishID: item.bpfcEstablishID,
          glues: item.glues || [],
          isGenerate: item.isGenerate
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
  delete(id) {
    this.alertify.confirm('Delete Plan <br> Xóa kế hoạc làm việc', 'Are you sure you want to delete this Plans ?<br> Bạn có chắc chắn muốn xóa không?', () => {
      this.planService.deleteRange([id]).subscribe(() => {
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
          id: item.id,
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
  openCloneModal(item) {
    this.modalReference = this.modalService.open(this.cloneModal);
    this.plansSelected = [{
      id: item.id,
      bpfcEstablishID: item.bpfcEstablishID,
      workingHour: item.workingHour,
      hourlyOutput: item.hourlyOutput,
      dueDate: item.dueDate,
      buildingID: item.buildingID
    }];
  }
  toolbarClick(args: any): void {
    switch (args.item.id) {
      case 'Clone':
        this.openModal(this.cloneModal);
        break;
      // case 'DeleteRange':
      //   if (this.grid.getSelectedRecords().length === 0) {
      //     this.alertify.warning('Hãy chọn 1 hoặc nhiều dòng để xóa <br>Please select the plans!!', true);
      //   } else {
      //     const selectedRecords = this.grid.getSelectedRecords().map((item: any) => {
      //       return item.id;
      //     });
      //     this.deleteRange(selectedRecords);
      //   }
      //   break;
      case 'Update':
        this.generateTodolist();
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
    this.planService.search(this.buildingID, startDate.toDateString(), endDate.toDateString()).subscribe((res: any) => {
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
          startWorkingTime: new Date(item.startWorkingTime),
          finishWorkingTime: new Date(item.finishWorkingTime),
          startTime: item.startTime,
          endTime: item.endTime,
          bpfcEstablishID: item.bpfcEstablishID,
          isGenerate: item.isGenerate,
          glues: item.glues || []
        } as IPlan;
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
  tooltipContext(data) {
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
  tooltip(args: QueryCellInfoEventArgs) {
    if (args.column.field === 'bpfcName') {
      const data = args.data as any;
      const tooltip: Tooltip = new Tooltip({
        content: this.tooltipContext(data.glues)
      }, args.cell as HTMLTableCellElement);
    }
  }
  onClickFilter() {
    this.search(this.startDate, this.endDate);
  }
  generateTodolist() {
    const selectedRecords = this.grid.dataSource as any[];
    const data = selectedRecords.filter(x => x.isGenerate === false);
    if (data.length === 0) {
      this.alertify.warning(`Tất cả các kế hoạch làm việc đã được tạo nhiệm vụ!<br>
      All work plans have been created with tasks !`, true);
      return;
    }
    const plansSelected: number[] = data.map((item: any) => {
      return item.id;
    });
    this.todolistService.generateToDoList(plansSelected).subscribe((res: any) => {
      if (res.status) {
        this.alertify.success('Tạo nhiệm vụ thành công!<br>Success!', true);
        this.getAll();
      } else {
        this.alertify.error(res.message, true);
      }
    }, err => this.alertify.error(err, true));
  }
  // End API

  // modal
  openStationComponent(data) {
    const modalRef = this.modalService.open(StationComponent, { size: 'lg', backdrop: 'static', keyboard: false });
    modalRef.componentInstance.plan = data as IPlan;
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
  // end modal
}
