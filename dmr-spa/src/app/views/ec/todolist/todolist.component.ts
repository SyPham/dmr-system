import { AfterViewInit, Component, HostListener, OnDestroy, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { GridComponent, PageSettingsModel, RowDataBoundEventArgs } from '@syncfusion/ej2-angular-grids';
import { Subscription } from 'rxjs';
import { IBuilding } from 'src/app/_core/_model/building';
import { DispatchParams, IMixingInfo } from 'src/app/_core/_model/plan';
import { IRole } from 'src/app/_core/_model/role';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { BuildingService } from 'src/app/_core/_service/building.service';
import { PlanService } from 'src/app/_core/_service/plan.service';
import { DispatchComponent } from '../dispatch/dispatch.component';
import { PrintGlueComponent } from '../print-glue/print-glue.component';
import { EmitType } from '@syncfusion/ej2-base/';
import { Query } from '@syncfusion/ej2-data/';
import { FilteringEventArgs } from '@syncfusion/ej2-angular-dropdowns';
import * as signalr from '../../../../assets/js/ec-client.js';
import { HubConnectionState } from '@microsoft/signalr';
import { TranslateService } from '@ngx-translate/core';
import { DataService } from 'src/app/_core/_service/data.service';
import { TodolistService } from 'src/app/_core/_service/todolist.service';
import { IToDoList, IToDoListForCancel } from 'src/app/_core/_model/IToDoList';
import { ClickEventArgs, ToolbarComponent } from '@syncfusion/ej2-angular-navigations';
import { ActivatedRoute, Router } from '@angular/router';
import { Button } from '@syncfusion/ej2-angular-buttons';
import { NgxSpinnerService } from 'ngx-spinner';
declare var $: any;
const ADMIN = 1;
const SUPERVISOR = 2;
const BUILDING_LEVEL = 2;

@Component({
  selector: 'app-todolist',
  templateUrl: './todolist.component.html',
  styleUrls: ['./todolist.component.css']
})
export class TodolistComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('toolbarTodo') toolbarTodo: ToolbarComponent;
  @ViewChild('toolbarDone') toolbarDone: ToolbarComponent;
  @ViewChild('toolbarDelay') toolbarDelay: ToolbarComponent;
  @ViewChild('gridDone') gridDone: GridComponent;
  @ViewChildren('tooltip') tooltip: QueryList<any>;

  @ViewChild('gridTodo') gridTodo: GridComponent;
  @ViewChild('gridDelay') gridDelay: GridComponent;
  focusDone: string;
  @ViewChild('fullScreen') divRef;
  sortSettings: object;
  pageSettings: PageSettingsModel;
  toolbarOptions: object;
  editSettings: object;
  searchSettings: any = { hierarchyMode: 'Parent' };
  fieldsBuildings: object = { text: 'name', value: 'id' };
  setFocus: any;
  data: IToDoList[];
  doneData: IToDoList[];
  building: IBuilding;
  role: IRole;
  buildingID: number;
  isShowTodolistDone: string;
  subscription: Subscription[] = [];
  IsAdmin: boolean;
  buildings: IBuilding[];
  buildingName: any;
  glueName: any;
  todoTotal = 0;
  doneTotal = 0;
  hasFullScreen: boolean;
  total = 0;
  percentageOfDone = 0;
  public mediaBtn: any;
  delayTotal = 0;
  TODO = 'todo';
  DONE = 'done';
  DELAY = 'delay';
  @HostListener('fullscreenchange', ['$event']) fullscreenchange(e) {
    if (document.fullscreenElement) {
      this.mediaBtn.iconCss = 'fas fa-compress-arrows-alt';
      this.mediaBtn.content = 'CloseScreen';
      console.log(`Element: ${document.fullscreenElement.id} entered fullscreen mode.`);
    } else {
      this.mediaBtn.iconCss = 'fa fa-expand-arrows-alt';
      this.mediaBtn.content = 'FullScreen';
      console.log('Leaving full-screen mode.');
    }
  }
  constructor(
    private planService: PlanService,
    private buildingService: BuildingService,
    private alertify: AlertifyService,
    public modalService: NgbModal,
    public dataService: DataService,
    private route: ActivatedRoute,
    private router: Router,
    private spinner: NgxSpinnerService,
    public todolistService: TodolistService
  ) { }
  ngOnDestroy() {
    this.subscription.forEach(subscription => subscription.unsubscribe());
  }
  ngAfterViewInit() {
    this.checkRole();
  }
  ngOnInit() {
    this.hasFullScreen = false;
    this.focusDone = this.TODO;
    if (signalr.CONNECTION_HUB.state === HubConnectionState.Connected) {
      signalr.CONNECTION_HUB.on(
        'ReceiveTodolist',
        (buildingID: number) => {
          if (this.buildingID === buildingID) {
            this.buildingID = buildingID;
            this.todo();
          }
        }
      );
    }
    this.IsAdmin = false;
    const BUIDLING: IBuilding = JSON.parse(localStorage.getItem('building'));
    const ROLE: IRole = JSON.parse(localStorage.getItem('level'));
    this.role = ROLE;
    this.building = BUIDLING;
    // this.buildingID = this.building.id;
    this.isShowTodolistDone = this.TODO;
    this.gridConfig();
    this.subscription.push(this.todolistService.getValue().subscribe(status => {
      if (status !== null ) {
        this.buildingID = this.building.id;
        this.todo();
      }
    }));
  }
  onRouteChange() {
    this.route.data.subscribe(data => {
      this.glueName = this.route.snapshot.params.glueName;
      this.gridTodo.search(this.glueName);
    });
  }
  getBuilding(callback): void {
    this.buildingService.getBuildings().subscribe(async (buildingData) => {
      this.buildings = buildingData.filter(item => item.level === BUILDING_LEVEL);
      callback();
    });
  }
  cancelRange(): void {
    const data = this.gridTodo.getSelectedRecords() as IToDoList[];
    const model: IToDoListForCancel[] = data.map(item => {
      const todo: IToDoListForCancel = {
        id: item.id,
        lineNames: item.lineNames
      };
      return todo;
    });
    this.todolistService.cancelRange(model).subscribe( (res) => {
    this.alertify.success('Xóa thành công! <br> Success!');
    });
  }
  cancel(todo: IToDoList): void {
    this.alertify.confirm('Cancel', 'Bạn có chắc chắn muốn hủy keo này không? Are you sure you want to get rid of this data?', () => {
      const model: IToDoListForCancel = {
        id: todo.id,
        lineNames: todo.lineNames
      };
      this.todolistService.cancel(model).subscribe((res) => {
        this.todo();
        this.alertify.success('Xóa thành công! <br> Success!');
      });

    });
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
    this.todo();
  }
  checkRole(): void {
    const roles = [ADMIN, SUPERVISOR];
    if (roles.includes(this.role.id)) {
      this.IsAdmin = true;
      const buildingId = +localStorage.getItem('buildingID');
      if (buildingId === 0) {
        this.alertify.message('Please select a building!', true);
        this.getBuilding(() => {});
      } else {
        this.getBuilding(() => {
          this.buildingID = buildingId;
          this.todo();
        });
      }
    } else {
      this.getBuilding(() => {
        this.buildingID = this.building.id;
        this.todo();
      });
    }
  }
  todo() {
    this.todolistService.todo(this.buildingID).subscribe(res => {
      this.data = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.total = res.total;
      this.delayTotal = res.delayTotal;
      this.percentageOfDone = res.percentageOfDone;
    });
  }
  delay() {
    this.todolistService.delay(this.buildingID).subscribe(res => {
      this.data = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.delayTotal = res.delayTotal;
      this.total = res.total;
      this.percentageOfDone = res.percentageOfDone;
    });
  }
  done() {
    this.todolistService.done(this.buildingID).subscribe(res => {
      this.doneData = res.data;
      this.todoTotal = res.todoTotal;
      this.doneTotal = res.doneTotal;
      this.delayTotal = res.delayTotal;
      this.percentageOfDone = res.percentageOfDone;
      this.total = res.total;
    });
  }
  gridConfig(): void {
    this.pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
    this.sortSettings = {};
    this.editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
    this.toolbarOptions = ['Search'];
  }
  dataBoundDone() {
    this.gridDone.autoFitColumns();
  }
  dataBound() {
  }
  createdTodo() {
    this.mediaBtn = new Button(
      { cssClass: `e-tbar-btn e-tbtn-txt e-control e-btn e-lib`,
        iconCss: 'fa fa-expand-arrows-alt',
        isToggle: true
      });
    this.mediaBtn.appendTo('#screen');
    this.mediaBtn.element.onclick = (): void => {
      if (this.mediaBtn.content === 'CloseScreen') {
        this.mediaBtn.iconCss = 'fa fa-expand-arrows-alt';
        this.mediaBtn.content = 'FullScreen';
        this.closeFullscreen();
      } else {
        this.openFullscreen();
        this.mediaBtn.iconCss = 'fas fa-compress-arrows-alt';
        this.mediaBtn.content = 'CloseScreen';
      }
    };
    const todoButton: HTMLElement = (this.toolbarTodo.element as HTMLElement).querySelector('#todo');
    todoButton?.classList.add('done');

  }
  createdDone() {
    this.mediaBtn = new Button(
      {
        cssClass: `e-tbar-btn e-tbtn-txt e-control e-btn e-lib`,
        iconCss: 'fa fa-expand-arrows-alt',
        isToggle: true
      });
    this.mediaBtn.appendTo('#screen');
    this.mediaBtn.element.onclick = (): void => {
      if (this.mediaBtn.content === 'CloseScreen') {
        this.mediaBtn.iconCss = 'fa fa-expand-arrows-alt';
        this.mediaBtn.content = 'FullScreen';
        this.closeFullscreen();
      } else {
        this.openFullscreen();
        this.mediaBtn.iconCss = 'fas fa-compress-arrows-alt';
        this.mediaBtn.content = 'CloseScreen';
      }
    };
    const doneButton: HTMLElement = (this.toolbarDone.element as HTMLElement).querySelector('#done');
    doneButton?.classList.add('done');

  }
  createdDelay() {
    this.mediaBtn = new Button(
      {
        cssClass: `e-tbar-btn e-tbtn-txt e-control e-btn e-lib`,
        iconCss: 'fa fa-expand-arrows-alt',
        isToggle: true
      });
    this.mediaBtn.appendTo('#screen');
    this.mediaBtn.element.onclick = (): void => {
      if (this.mediaBtn.content === 'CloseScreen') {
        this.mediaBtn.iconCss = 'fa fa-expand-arrows-alt';
        this.mediaBtn.content = 'FullScreen';
        this.closeFullscreen();
      } else {
        this.openFullscreen();
        this.mediaBtn.iconCss = 'fas fa-compress-arrows-alt';
        this.mediaBtn.content = 'CloseScreen';
      }
    };
    const delayButton: HTMLElement = (this.toolbarDelay.element as HTMLElement).querySelector('#delay');
    delayButton?.classList.add('done');
  }
  searchDone(args) {
    if (this.focusDone === this.DONE) {
      this.gridDone.search(args.target.value);
    } else if (this.focusDone === this.TODO) {
      this.gridTodo.search(args.target.value);
    } else if (this.focusDone === this.DELAY) {
      this.gridDelay.search(args.target.value);
    }
  }
  onClickToolbar(args: ClickEventArgs): void {
    // debugger;
    const target: HTMLElement = (args.originalEvent.target as HTMLElement).closest('button'); // find clicked button
    switch (target?.id) {
      case 'done':
        this.isShowTodolistDone = this.DONE;
        this.focusDone = this.DONE;
        this.done();
        // target.focus();
        break;
      case 'todo':
        this.isShowTodolistDone = this.TODO;
        this.focusDone = this.TODO;
        this.todo();
        // target.focus();
        break;
      case 'delay':
        this.isShowTodolistDone = this.DELAY;
        this.focusDone = this.DELAY;
        this.delay();
        // target.focus();
        break;
      case 'excelExport':
        this.spinner.show();
        this.todolistService.exportExcel(this.building.id).subscribe((data: any) => {
          const blob = new Blob([data],
            { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

          const downloadURL = window.URL.createObjectURL(data);
          const link = document.createElement('a');
          link.href = downloadURL;
          link.download = 'doneListReport.xlsx';
          link.click();
          this.spinner.hide();
        });
        break;
      default:
        break;
    }
  }
  toolbarClick(args: any): void {
    switch (args.item.id) {
      case 'Done':
        this.isShowTodolistDone = this.DONE;
        this.done();
        break;
      case 'Undone':
        this.isShowTodolistDone = this.TODO;
        this.todo();
        break;
      case 'Delay':
        this.isShowTodolistDone = this.DELAY;
        this.todo();
        break;
      default:
        break;
    }
  }
  actionComplete(e) {
    if (e.requestType === 'beginEdit') {
      if (this.setFocus?.field) {
        e.form.elements.namedItem('quantity').focus(); // Set focus to the Target element
      }
    }
  }
  onDoubleClick(args: any): void {
    this.setFocus = args.column; // Get the column from Double click event
  }
  rowDB(args: RowDataBoundEventArgs): void {
    const data = args.data as any;
  }
  onDoubleClickDone(args: any): void {
    if (args.column.field === 'deliveredConsumption') {
      const value = args.rowData as IToDoList;
      this.openDispatchModal(value);
    }
  }
  actionBegin(args) {
    if (args.requestType === 'cancel') {
    }

    if (args.requestType === 'save') {
      if (args.action === 'edit') {
        const previousData = args.previousData;
        const data = args.data;
        if (data.quantity !== previousData.quantity) {
          const planId = args.data.id || 0;
          const quantity = args.data.quantity;
        } else { args.cancel = true; }
      }
    }
  }
  onBeforeRender(args, data, i) {
    const t = this.tooltip.filter((item, index) => index === +i)[0];
    t.content = 'Loading...';
    t.dataBind();
    this.planService
      .getBPFCByGlue(data.glueName)
      .subscribe((res: any) => {
        t.content = res.join('<br>');
        t.dataBind();
      });
  }

  // modal
  goToStir(data: IToDoList) {
    if (data.finishMixingTime === null) {
      this.alertify.warning('Hãy thực hiện bước trộn keo trước!', true);
      return;
    }
    this.router.navigate(['/ec/execution/todolist-2/stir/' + data.mixingInfoID + '/' + data.glueName]);
  }
  openDispatchModal(value: IToDoList) {
    if (value.printTime === null && value.glueName.includes(' + ')) {
      this.alertify.warning('Hãy thực hiện bước in keo trước!', true);
      return;
    }
    if (value.mixingInfoID === 0 && !value.glueName.includes(' + '))
    { this.alertify.warning('Hãy thực hiện bước in keo trước!<br> Please perform glue printing first!', true); return; }
    const modalRef = this.modalService.open(DispatchComponent, { size: 'xl', backdrop: 'static', keyboard : false });
    modalRef.componentInstance.value = value;
    modalRef.result.then((result) => {
    }, (reason) => {
        this.todolistService.setValue(true);
    });
  }
  openPrintModal(value: IToDoList) {
    // if (value.finishStirTime === null && value.glueName.includes(' + ')) {
    //   this.alertify.warning('Hãy thực hiện bước khuấy keo trước!', true);
    //   return;
    // }
    this.todolistService.findPrintGlue(value.mixingInfoID).subscribe(data => {
      if (data?.id === 0 && value.glueName.includes(' + ')) {
        this.alertify.error('Please mixing this glue first!', true);
        return;
      }
      const modalRef = this.modalService.open(PrintGlueComponent, { size: 'md', backdrop: 'static', keyboard: false  });
      modalRef.componentInstance.data = data;
      modalRef.componentInstance.value = value;
      modalRef.result.then((result) => {
      }, (reason) => {
      });
    });
  }
  lockDispatch(data: IToDoList): string {
    let classList = '';
    if (data.deliveredConsumption === data.mixedConsumption && data.mixedConsumption > 0 && data.deliveredConsumption > 0) {
      classList = 'disabled cursor-pointer';
    }
    return classList;
  }
  // config screen
  openFullscreen() {
    // Use this.divRef.nativeElement here to request fullscreen
    const elem = this.divRef.nativeElement;
    if (elem.requestFullscreen) {
      elem.requestFullscreen();
    } else if (elem.msRequestFullscreen) {
      elem.msRequestFullscreen();
    } else if (elem.mozRequestFullScreen) {
      elem.mozRequestFullScreen();
    } else if (elem.webkitRequestFullscreen) {
      elem.webkitRequestFullscreen();
    }
    this.hasFullScreen = true;
  }
  closeFullscreen() {
    if (document.exitFullscreen) {
      document.exitFullscreen();
    } else if ((document as any).mozCancelFullScreen) {
      (document as any).mozCancelFullScreen();
    } else if ((document as any).webkitExitFullscreen) {
      (document as any).webkitExitFullscreen();
    } else if ((document as any).msExitFullscreen) {
      (window.top.document as any).msExitFullscreen();
    }
    this.hasFullScreen = false;
  }
}
