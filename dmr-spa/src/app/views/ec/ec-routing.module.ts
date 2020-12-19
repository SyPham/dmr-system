import { StirComponent } from './stir/stir.component';
import { AbnormalListComponent } from './abnormal-list/abnormal-list.component';
import { SearchComponent } from './search/search.component';
import { InventoryComponent } from './inventory/inventory.component';
import { SuppilerComponent } from './suppiler/suppiler.component';
import { PlanComponent } from './plan/plan.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { GlueComponent } from './glue/glue.component';
import { IngredientComponent } from './ingredient/ingredient.component';
import { GlueResolver } from '../../_core/_resolvers/glue.resolver';
import { BuildingComponent } from './building/building.component';
import { BuildingUserComponent } from './building-user/building-user.component';
import { SummaryComponent } from './summary/summary.component';
import { AccountComponent } from './account/account.component';
import { BPFCScheduleComponent } from './BPFCSchedule/BPFCSchedule.component';
import { PrintQRCodeComponent } from './ingredient/print-qrcode/print-qrcode.component';
import { PartComponent } from './part/part.component';
import { KindComponent } from './kind/kind.component';
import { MaterialComponent } from './material/material.component';
import { BpfcComponent } from './bpfc/bpfc.component';
import { BpfcStatusComponent } from './bpfc-status/bpfc-status.component';
import { GlueHistoryComponent } from './summary/glue-history/glue-history.component';
import { DeliveredHistoryComponent } from './delivered-history/delivered-history.component';
import { IncomingComponent } from './incoming/incoming.component';
import { BuildingSettingComponent } from './building-setting/building-setting.component';
import { PlanOutputQuantityComponent } from './plan-output-quantity/plan-output-quantity.component';
import { CostingComponent } from './costing/costing.component';
import { ConsumptionComponent } from './consumption/consumption.component';
import { ScalingSettingComponent } from './scaling-setting/scaling-setting.component';
import { Consumption1Component } from './consumption-1/consumption-1.component';
import { Consumption2Component } from './consumption-2/consumption-2.component';
import { TodolistComponent } from './todolist/todolist.component';
import { MixingComponent } from './mixing/mixing.component';
import { DispatchComponent } from './dispatch/dispatch.component';
import { BpfcDetailComponent } from './bpfc-detail/bpfc-detail.component';
import { Bpfc1Component } from './bpfc-1/bpfc-1.component';
import { GlueTypeComponent } from './glue-type/glue-type.component';

const routes: Routes = [
  {
    path: '',
    data: {
      title: 'ec',
      breadcrumb: 'Home'
    },
    children: [

      // setting
      {
        path: 'setting',
        data: {
          title: 'setting',
          breadcrumb: 'Setting'
        },
        children: [
          {
            path: '',
            component: IngredientComponent,
          },
          {
            path: 'ingredient',
            // component: IngredientComponent,
            data: {
              title: 'Ingredient',
              breadcrumb: 'Ingredient',
              url: 'ec/setting/Ingredient'
            },
            children: [
              {
                path: '',
                component: IngredientComponent,
              },
              {
                path: 'print-qrcode/:id/:code/:name',
                component: PrintQRCodeComponent,
                data: {
                  title: 'Print QRCode',
                  breadcrumb: 'Print QRCode'
                }
              }
            ]
          },
          {
            path: 'account-1',
            component: AccountComponent,
            data: {
              title: 'account',
              breadcrumb: 'Account'
            }
          },
          {
            path: 'account-2',
            component: BuildingUserComponent,
            data: {
              title: 'Account 2',
              breadcrumb: 'Account 2'
            }
          },
          {
            path: 'building-setting',
            component: BuildingSettingComponent,
            data: {
              title: 'Building Setting',
              breadcrumb: 'Building Setting'
            }
          },
          {
            path: 'glue-type',
            component: GlueTypeComponent,
            data: {
              title: 'Glue Type',
              breadcrumb: 'Glue Type'
            }
          },
          {
            path: 'scaling-setting',
            component: ScalingSettingComponent,
            data: {
              title: 'Scaling Setting',
              breadcrumb: 'Scaling Setting'
            }
          },
          {
            path: 'costing',
            component: CostingComponent,
            data: {
              title: 'costing',
              breadcrumb: 'Costing'
            }
          },
          {
            path: 'building',
            component: BuildingComponent,
            data: {
              title: 'Building',
              breadcrumb: 'Building'
            }
          },
          {
            path: 'supplier',
            component: SuppilerComponent,
            data: {
              title: 'Suppiler',
              breadcrumb: 'Suppiler'
            }
          },
          {
            path: 'kind',
            component: KindComponent,
            data: {
              title: 'Kind',
              breadcrumb: 'Kind'
            }
          },
          {
            path: 'part',
            component: PartComponent,
            data: {
              title: 'Part',
              breadcrumb: 'Part'
            }
          },
          {
            path: 'material',
            component: MaterialComponent,
            data: {
              title: 'Material',
              breadcrumb: 'Material'
            }
          },
          {
            path: 'glue',
            component: GlueComponent,
            resolve: { glues: GlueResolver },
            data: {
              title: 'Glue',
              breadcrumb: 'Glue'
            }
          },
        ]
      },
       // end setting

      {
        path: 'troubleshooting',
        data: {
          title: 'Troubleshooting',
          breadcrumb: 'Troubleshooting'
        },
        children: [
          {
            path: 'search',
            component: SearchComponent,
            data: {
              title: 'Troubleshooting Search',
              breadcrumb: 'Search'
            }
          },
          {
            path: 'Abnormal-List',
            component: AbnormalListComponent,
            data: {
              title: 'Troubleshooting Black List',
              breadcrumb: 'Abnormal-List'
            }
          },
        ]
      },

      // establish
      {
        path: 'establish',
        data: {
          title: 'Establish',
          breadcrumb: 'Establish'
        },
        children: [
          // {
          //   path: 'bpfc-schedule',
          //   component: BPFCScheduleComponent,
          //   data: {
          //     title: 'BPFC Schedule',
          //     breadcrumb: 'BPFC Schedule'
          //   }
          // },
          {
            path: 'bpfc-schedule',
            // resolve: { glues: GlueResolver },
            // component: BpfcComponent,
            data: {
              title: 'bpfc-schedule',
              breadcrumb: 'BPFC Schedule'
            },
            children: [
              {
                path: '',
                component: BPFCScheduleComponent
              },
              {
                path: 'detail',
                data: {
                  title: 'Detail',
                  breadcrumb: 'Detail'
                },
                children: [
                  {
                    path: '',
                    component: BpfcDetailComponent,
                  },
                  {
                    path: ':id',
                    component: BpfcDetailComponent,
                    // data: {
                    //   breadcrumb: ''
                    // }
                  }
                ]
              },

            ]
          },
          {
            path: 'bpfc-status',
            component: BpfcStatusComponent,
            data: {
              title: 'BPFC Status',
              breadcrumb: 'BPFC Status'
            }
          },
          {
            path: 'bpfc-1',
            component: Bpfc1Component,
            data: {
              title: 'BPFC ',
              breadcrumb: 'BPFC '
            }
          }

        ]
      },
      // establish
      // {
      //   path: 'establish',
      //   data: {
      //     title: 'Establish',
      //     breadcrumb: 'Establish'
      //   },
      //   children: [
      //     {
      //       path: 'bpfc-schedule',
      //       component: BPFCScheduleComponent,
      //       data: {
      //         title: 'BPFC Schedule',
      //         breadcrumb: 'BPFC Schedule'
      //       }
      //     },
      //     {
      //       path: 'bpfc',
      //       resolve: { glues: GlueResolver },
      //       component: BpfcComponent,
      //       data: {
      //         title: 'bpfc',
      //         breadcrumb: 'BPFC'
      //       }
      //     },
      //     {
      //       path: 'bpfc-status',
      //       component: BpfcStatusComponent,
      //       data: {
      //         title: 'BPFC Status',
      //         breadcrumb: 'BPFC Status'
      //       }
      //     }
      //   ]
      // },
      // end establish

      // execution
      {
        path: 'execution',
        data: {
          title: 'Execution',
          breadcrumb: 'Execution'
        },
        children: [
          {
            path: 'todolist',
            data: {
              title: 'todolist',
              breadcrumb: 'Todolist'
            },
            children: [
              {
                path: '',
                component: SummaryComponent,
              },
              {
                path: 'stir',
                component: StirComponent,
                data: {
                  title: 'Stir',
                  breadcrumb: 'Stir'
                }
              },
              {
                path: 'stir/:glueName',
                component: StirComponent,
                data: {
                  title: 'Stir',
                  breadcrumb: 'Stir'
                }
              },
              {
                path: 'history/:glueID',
                component: GlueHistoryComponent,
                data: {
                  title: 'History',
                  breadcrumb: 'History'
                }
              },
            ]
          },
          {
            path: 'todolist-2',
            data: {
              title: 'todolist-2',
              breadcrumb: 'To Do List 2'
            },
            children: [
              {
                path: '',
                component: TodolistComponent,
              },
              {
                path: ':glueName',
                component: TodolistComponent,
              },
              {
                path: 'stir',
                component: StirComponent,
                data: {
                  title: 'Stir',
                  breadcrumb: 'Stir'
                }
              },
              {
                path: 'stir/:mixingInfoID/:glueName',
                component: StirComponent,
                data: {
                  title: 'Stir',
                  breadcrumb: 'Stir'
                }
              },
              {
                path: 'mixing',
                component: MixingComponent,
                data: {
                  title: 'Mixing',
                  breadcrumb: 'Mixing'
                }
              },
              {
                path: 'mixing/:glueID/:estimatedStartTime/:estimatedFinishTime/:stdcon',
                component: MixingComponent,
                data: {
                  title: 'Mixing',
                  breadcrumb: 'Mixing'
                }
              },
              {
                path: 'dispatch',
                component: DispatchComponent,
                data: {
                  title: 'dispatch',
                  breadcrumb: 'Dispatch'
                }
              },
              {
                path: 'dispatch/:mixingInfoID',
                component: DispatchComponent,
                data: {
                  title: 'dispatch',
                  breadcrumb: 'Dispatch'
                }
              },
            ]
          },
          {
            path: 'workplan',
            component: PlanComponent,
            data: {
              title: 'Workplan',
              breadcrumb: 'Work Plan'
            }
          },
          {
            path: 'output-quantity',
            component: PlanOutputQuantityComponent,
            data: {
              title: 'Output Quantity',
              breadcrumb: 'Output Quantity'
            }
          },
          {
            path: 'incoming',
            component: IncomingComponent,
            data: {
              title: 'Stock',
              breadcrumb: 'Stock'
            }
          }
        ]
      },
      // end execution

       // report
      {
        path: 'report',
        data: {
          title: 'Report',
          breadcrumb: 'Report'
        },
        children: [
          {
            path: 'consumption',
            component: ConsumptionComponent,
            data: {
              title: 'Cost',
              breadcrumb: 'Cost'
            }
          },
          {
            path: 'consumption-1',
            component: Consumption1Component,
            data: {
              title: 'Consumption 1',
              breadcrumb: 'Consumption 1'
            }
          },
          {
            path: 'consumption-2',
            component: Consumption2Component,
            data: {
              title: 'Consumption 2',
              breadcrumb: 'Consumption 2'
            }
          },
          {
            path: 'delivered-history',
            component: DeliveredHistoryComponent,
            data: {
              title: 'Delivered History',
              breadcrumb: 'Delivered History'
            }
          },
          {
            path: 'inventory',
            component: InventoryComponent,
            data: {
              title: 'Inventory',
              breadcrumb: 'Inventory'
            },
          }
        ]
      },
      // end report
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ECRoutingModule { }
