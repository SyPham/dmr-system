import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { GlueTypeService } from 'src/app/_core/_service/glue-type.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ToolbarItems, TreeGridComponent } from '@syncfusion/ej2-angular-treegrid';
import { AlertifyService } from 'src/app/_core/_service/alertify.service';
import { SortService, FilterService, ReorderService, ITreeData } from '@syncfusion/ej2-angular-treegrid';
import { HierarchyNode, IGlueType } from 'src/app/_core/_model/glue-type';
import { GlueTypeModalComponent } from './glue-type-modal/glue-type-modal.component';
import { RowDataBoundEventArgs } from '@syncfusion/ej2-angular-grids';
import { BeforeOpenCloseEventArgs } from '@syncfusion/ej2-inputs';
@Component({
  selector: 'app-glue-type',
  templateUrl: './glue-type.component.html',
  styleUrls: ['./glue-type.component.css'],
  providers: [FilterService, SortService, ReorderService],
  encapsulation: ViewEncapsulation.None
})
export class GlueTypeComponent implements OnInit {
  toolbarOptions: ToolbarItems[];
  data: HierarchyNode<IGlueType>[];
  editing: any;

  contextMenuItems: any;
  pageSettings: any;
  editparams: { params: { format: string; }; };
  @ViewChild('treegrid')
  public treeGridObj: TreeGridComponent;
  @ViewChild('buildingModal')
  buildingModal: any;
  glueType: IGlueType;
  edit: IGlueType;
  constructor(
    private buildingService: GlueTypeService,
    private modalService: NgbModal,
    private alertify: AlertifyService,
  ) { }

  ngOnInit() {
    this.editing = { allowDeleting: true, allowEditing: true, mode: 'Row' };
    // this.toolbarOptions = ['Add', 'Delete', 'Search', 'Update', 'Cancel'];
    this.optionTreeGrid();
    this.onService();
    this.getGlueTypesAsTreeView();
  }
  optionTreeGrid() {
    this.contextMenuItems = [
      {
        text: 'Add Child',
        iconCss: ' e-icons e-add',
        target: '.e-content',
        id: 'Add-Sub-Item'
      },
      {
        text: 'Delete',
        iconCss: ' e-icons e-delete',
        target: '.e-content',
        id: 'DeleteOC'
      }
    ];
    this.toolbarOptions = [
      'Add',
      'Delete',
      'Search',
      'ExpandAll',
      'CollapseAll',
      'ExcelExport',
      'PdfExport'
    ];
    this.editing = { allowEditing: true, allowAdding: true, allowDeleting: true, mode: 'Row' };
    this.pageSettings = { pageSize: 20 };
    this.editparams = { params: { format: 'n' } };
  }
  created() {
    this.getGlueTypesAsTreeView();
  }
  onService() {
    this.buildingService.currentMessage
      .subscribe(arg => {
        if (arg === 200) {
          this.getGlueTypesAsTreeView();
        }
      });
  }
  toolbarClick(args) {
    switch (args.item.text) {
      case 'Add':
        args.cancel = true;
        this.openMainModal();
        break;
      case 'PDF Export':
        this.treeGridObj.pdfExport({ hierarchyExportMode: 'All' });
        break;
      case 'Excel Export':
        this.treeGridObj.excelExport({ hierarchyExportMode: 'All' });
        break;
      default:
        break;
    }
  }
  contextMenuOpen(arg): void {
    const data = arg.rowInfo.rowData as HierarchyNode<IGlueType>;
    if (data.entity.level === 2) {
      arg.cancel = true;
    }
  }
  contextMenuClick(args) {
    const data = args.rowInfo.rowData.entity as HierarchyNode<IGlueType>;
    switch (args.item.id) {
      case 'DeleteOC':
        this.delete(data.entity.id);
        break;
      case 'Add-Sub-Item':
        this.openSubModal();
        break;
      default:
        break;
    }
  }
  delete(id) {
    this.alertify.confirm(
      'Delete Glue Type',
      'Are you sure you want to delete this GlueTypeID "' + id + '" ?',
      () => {
        this.buildingService.delete(id).subscribe(res => {
          this.getGlueTypesAsTreeView();
          this.alertify.success('The glue-type has been deleted!!!');
        },
        error => {
          this.alertify.error('Failed to delete the glue-type!!!');
        });
      }
    );
   }
  actionBegin(args) {
    if (args.requestType === 'save' && args.action === 'edit') {
      const data = args.data as HierarchyNode<IGlueType>;
      this.edit.title = data.entity.title;
      this.edit.minutes = data.entity.minutes;
      this.edit.rpm = data.entity.rpm;
      this.edit.level = data.entity.level;
      this.edit.id = data.entity.id;
      this.edit.parentID = data.entity.parentID;
      this.rename();
    }
    if (args.requestType === 'delete') {
      const data = args.data[0] as HierarchyNode<IGlueType>;
      this.delete(data.entity.id);
    }
   }
  rowSelected(args) {
    this.edit = {
      id: args.data.entity.id,
      title: args.data.entity.title,
      level: args.data.entity.level,
      parentID: args.data.entity.parentID,
      rpm: args.data.entity.rpm,
      minutes: args.data.entity.minutes,
    };
    this.glueType = {
      id: 0,
      title: '',
      parentID: args.data.entity.id,
      level: 0,
      rpm: args.data.entity.rpm,
      minutes: args.data.entity.minutes,
    };
  }
  getGlueTypesAsTreeView() {
    this.buildingService.getGlueTypesAsTreeView().subscribe(res => {
      this.data = res;
    });
  }
  rowDB(args: RowDataBoundEventArgs) {
    const data = args.data as IGlueType;
    if (data.level === 1) {
      // args.row.classList.add('bgcolor');
    }
  }
  clearFrom() {
    this.glueType = {
      id: 0,
        title: '',
      parentID: 0,
      level: 0,
      rpm: 0,
      minutes: 0,
    };
  }
  queryCellInfo(args) {
    if (args.column.field === 'entity.title') {
      if (args.data.entity.level === 1) {
        args.colSpan = 4;
        // merging 2 columns of same row using colSpan property
      }
    }
  }
  rename() {
    this.buildingService.update(this.edit).subscribe(res => {
      this.getGlueTypesAsTreeView();
      this.alertify.success('The glue-type has been changed!!!');
    });
  }
  openMainModal() {
    this.clearFrom();
    const modalRef = this.modalService.open(GlueTypeModalComponent, { size: 'lg' });
    modalRef.componentInstance.title = 'Add Glue Type Parent';
    modalRef.componentInstance.glueType = this.glueType;
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
  openSubModal() {
    const modalRef = this.modalService.open(GlueTypeModalComponent, { size: 'lg' });
    modalRef.componentInstance.title = 'Add Glue Type Child';
    modalRef.componentInstance.glueType = this.glueType;
    modalRef.result.then((result) => {
    }, (reason) => {
    });
  }
}
