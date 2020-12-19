import { AfterViewInit, Component, OnDestroy, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { GridComponent, PageSettingsModel } from '@syncfusion/ej2-angular-grids';
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
import { ActivatedRoute } from '@angular/router';

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
  @ViewChild('gridDone') gridDone: GridComponent;
  @ViewChildren('tooltip') tooltip: QueryList<any>;

  @ViewChild('gridTodo') gridTodo: GridComponent;
  focusDone: boolean;
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
  isShowTodolistDone: boolean;
  subscription: Subscription[] = [];
  IsAdmin: boolean;
  buildings: IBuilding[];
  buildingName: any;
  glueName: any;
  constructor(
    private planService: PlanService,
    private buildingService: BuildingService,
    private alertify: AlertifyService,
    public modalService: NgbModal,
    public dataService: DataService,
    private route: ActivatedRoute,
    public todolistService: TodolistService
  ) { }
  ngOnDestroy() {
    this.subscription.forEach(subscription => subscription.unsubscribe());
  }
  ngAfterViewInit() {
  }
  ngOnInit() {
    this.focusDone = false;
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
    this.isShowTodolistDone = false;
    this.gridConfig();
    this.checkRole();
    this.subscription.push(this.todolistService.getValue().subscribe(status => {
      if (status === true) {
        this.todo();
        return;
      } else if (status === false) {
        this.done();
        return;
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
    this.todolistService.todo(this.buildingID).subscribe((res: IToDoList[]) => {
      this.data = res;
    });
  }
  done() {
    this.todolistService.done(this.buildingID).subscribe((res: IToDoList[]) => {
      this.doneData = res;
    });
  }
  gridConfig(): void {
    this.pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
    this.sortSettings = { columns: [{ field: 'dueDate', direction: 'Ascending' }] };
    this.editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
    this.toolbarOptions = ['Search'];
  }
  dataBoundDone() {
    this.gridDone.autoFitColumns();
  }
  dataBound() {
  }
  createdTodo() {
    if (this.toolbarTodo && this.focusDone === false) {
      const target: HTMLElement = (this.toolbarTodo.element as HTMLElement).querySelector('#todo');
      target?.focus();
    }
  }
  createdDone() {
    if (this.toolbarDone && this.focusDone === true) {
      const target: HTMLElement = (this.toolbarDone.element as HTMLElement).querySelector('#done');
      target?.focus();
    }
  }
  searchDone(args) {
    if (this.focusDone === true) {
      this.gridDone.search(args.target.value);
    } else {
      this.gridTodo.search(args.target.value);
    }
  }
  onClickToolbar(args: ClickEventArgs): void {
    // debugger;
    const target: HTMLElement = (args.originalEvent.target as HTMLElement).closest('button'); // find clicked button
    switch (target?.id) {
      case 'done':
        this.isShowTodolistDone = true;
        this.focusDone = true;
        this.done();
        target.focus();
        break;
      case 'todo':
        this.isShowTodolistDone = false;
        this.focusDone = false;
        this.todo();
        target.focus();
        break;
      default:
        break;
    }
  }
  toolbarClick(args: any): void {
    switch (args.item.id) {
      case 'Done':
        this.isShowTodolistDone = true;
        this.done();
        break;
      case 'Undone':
        this.isShowTodolistDone = false;
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
  openDispatchModal(value: IToDoList) {
    const modalRef = this.modalService.open(DispatchComponent, { size: 'xl', backdrop: 'static', keyboard : false });
    modalRef.componentInstance.value = value;
    modalRef.result.then((result) => {
    }, (reason) => {
      this.isShowTodolistDone = false;
      this.todo();
      this.gridTodo.refresh();
    });
  }
  openPrintModal(value: IToDoList) {
    this.todolistService.findPrintGlue(value.mixingInfoID).subscribe((data: any) => {
      if (data?.id === 0) {
        this.alertify.error('Please mixing this glue first!', true);
        return;
      }
      const modalRef = this.modalService.open(PrintGlueComponent, { size: 'xl', backdrop: 'static', keyboard: false  });
      modalRef.componentInstance.data = data;
      modalRef.result.then((result) => {
      }, (reason) => {
        this.isShowTodolistDone = true;
        this.done();
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
}
