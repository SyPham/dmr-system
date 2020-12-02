import { Component, OnDestroy, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { GridComponent, PageSettingsModel } from '@syncfusion/ej2-angular-grids';
import { Subscription } from 'rxjs';
import { IBuilding } from 'src/app/_core/_model/building';
import { DispatchParams, IMixingInfo, Todolist } from 'src/app/_core/_model/plan';
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

declare var $: any;
const ADMIN = 1;
const SUPERVISOR = 2;
const BUILDING_LEVEL = 2;
@Component({
  selector: 'app-todolist',
  templateUrl: './todolist.component.html',
  styleUrls: ['./todolist.component.css']
})
export class TodolistComponent implements OnInit, OnDestroy {
  @ViewChild('gridDone') gridDone: GridComponent;
  @ViewChildren('tooltip') tooltip: QueryList<any>;

  @ViewChild('gridUndone') gridUndone: GridComponent;
  sortSettings: object;
  pageSettings: PageSettingsModel;
  toolbarOptions: object;
  editSettings: object;
  searchSettings: any = { hierarchyMode: 'Parent' };
  fieldsBuildings: object = { text: 'name', value: 'id' };
  setFocus: any;
  data: Todolist[];
  doneData: Todolist[];
  building: IBuilding;
  role: IRole;
  buildingID: number;
  isShowTodolistDone: boolean;
  subscription: Subscription[] = [];
  IsAdmin: boolean;
  buildings: IBuilding[];
  buildingName: any;
  constructor(
    private planService: PlanService,
    private buildingService: BuildingService,
    private alertify: AlertifyService,
    public modalService: NgbModal,
    public dataService: DataService,
    private translate: TranslateService
  ) { }
  ngOnDestroy() {
    this.subscription.forEach(subscription => subscription.unsubscribe());
  }
  ngOnInit() {
    if (signalr.CONNECTION_HUB.state === HubConnectionState.Connected) {
      signalr.CONNECTION_HUB.on(
        'ReceiveTodolist',
        (buildingID: number) => {
          if (this.buildingID === buildingID) {
            this.buildingID = buildingID;
            this.todolist2();
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
    this.subscription.push(this.planService.getValue().subscribe(status => {
      if (status === true) {
        this.todolist2();
        return;
      } else if (status === false) {
        this.todolist2ByDone();
        return;
      }
    }));
  }
  getBuilding(callback): void {
    this.buildingService.getBuildings().subscribe(async (buildingData) => {
      this.buildings = buildingData.filter(item => item.level === BUILDING_LEVEL);
      callback();
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
    this.todolist2();
  }
  checkRole(): void {
    const roles = [ADMIN, SUPERVISOR];
    if (roles.includes(this.role.id)) {
      this.IsAdmin = true;
      const buildingId = +localStorage.getItem('buildingID');
      if (buildingId === 0) {
        this.alertify.message('Please select a building!', true);
      } else {
        this.getBuilding(() => {
          this.buildingID = buildingId;
          this.todolist2();
        });
      }
    } else {
      this.getBuilding(() => {
        this.buildingID = this.building.id;
        this.todolist2();
      });
    }
  }
  todolist2() {
    this.planService.todolist2(this.buildingID).subscribe((res: Todolist[]) => {
      this.data = res;
    });
  }
  todolist2ByDone() {
    this.planService.todolist2ByDone(this.buildingID).subscribe((res: Todolist[]) => {
      this.doneData = res;
    });
  }
  gridConfig(): void {
    this.pageSettings = { pageCount: 20, pageSizes: true, pageSize: 10 };
    this.sortSettings = { columns: [{ field: 'dueDate', direction: 'Ascending' }] };
    this.editSettings = { showDeleteConfirmDialog: false, allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Normal' };
    const doneEn = 'Done';
    const undoneEn = 'Undone';
    const doneVi = 'Đã Xong';
    const undoneVi = 'Chưa Xong';
    this.subscription.push(this.dataService.getValueLocale().subscribe(lang => {
      if (lang === 'vi') {
        this.toolbarOptions = [
          { text: doneVi, tooltipText: doneVi, prefixIcon: 'fa fa-remove', id: 'Undone' },
          { text: undoneVi, tooltipText: undoneVi, prefixIcon: 'fa fa-check', id: 'Done' },
          'Search'];
        return;
      } else if (lang === 'en') {
        this.toolbarOptions = [
          { text: doneEn, tooltipText: 'Undone', prefixIcon: 'fa fa-remove', id: 'Undone' },
          { text: undoneEn, tooltipText: 'Done', prefixIcon: 'fa fa-check', id: 'Done' },
          'Search'];
        return;
      } else {
        const langLocal = localStorage.getItem('lang');
        if (langLocal === 'vi') {
          this.toolbarOptions = [
            { text: doneVi, tooltipText: doneVi, prefixIcon: 'fa fa-remove', id: 'Undone' },
            { text: undoneVi, tooltipText: undoneVi, prefixIcon: 'fa fa-check', id: 'Done' },
            'Search'];
          return;
        } else if (langLocal === 'en') {
          this.toolbarOptions = [
            { text: doneEn, tooltipText: doneEn, prefixIcon: 'fa fa-remove', id: 'Undone' },
            { text: undoneEn, tooltipText: undoneEn, prefixIcon: 'fa fa-check', id: 'Done' },
            'Search'];
          return;
        }
      }
    }));
  }
  dataBound() {
    // this.grid.autoFitColumns();
  }
  toolbarClick(args: any): void {
    switch (args.item.id) {
      case 'Done':
        this.isShowTodolistDone = true;
        this.todolist2ByDone();
        break;
      case 'Undone':
        this.isShowTodolistDone = false;
        this.todolist2();
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
      .getBPFCByGlue(data.glue)
      .subscribe((res: any) => {
        t.content = res.join('<br>');
        t.dataBind();
      });
  }

  // modal
  openDispatchModal(value: Todolist) {
    const modalRef = this.modalService.open(DispatchComponent, { size: 'xl' });
    modalRef.componentInstance.value = value;
    modalRef.result.then((result) => {
    }, (reason) => {
      this.isShowTodolistDone = false;
      this.todolist2();
    });
  }
  openPrintModal(value: Todolist) {
    const obj: DispatchParams = {
      id: value.id,
      glue: value.glue,
      lines: value.lines,
      estimatedTime: value.estimatedTime,
    };
    this.planService.print(obj).subscribe((data: any) => {
      if (data?.id === 0) {
        this.alertify.error('Please mixing this glue first!', true);
        return;
      }
      const modalRef = this.modalService.open(PrintGlueComponent, { size: 'xl' });
      modalRef.componentInstance.data = data;
      modalRef.result.then((result) => {
      }, (reason) => {
        this.isShowTodolistDone = true;
        this.todolist2ByDone();
      });
    });
  }
}
