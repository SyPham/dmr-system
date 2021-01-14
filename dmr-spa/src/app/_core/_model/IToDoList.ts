import { IIngredient } from './summary';

export interface IToDoList {
    id: number;
    planID: number;
    mixingInfoID: number;
    glueID: number;
    buildingID: number;
    lineID: number;
    lineName: string;
    lineNames: string[];
    glueName: string;
    supplier: string;
    status: boolean;
    startMixingTime: Date;
    finishMixingTime: Date;
    startStirTime: Date;
    finishStirTime: Date;
    startDispatchingTime: Date;
    finishDispatchingTime: Date;
    printTime: Date;
    standardConsumption: number;
    mixedConsumption: number;
    deliveredConsumption: number;
    estimatedStartTime: Date;
    estimatedFinishTime: Date;
}
export interface IToDoListForCancel {
    id: number;
    lineNames: string[];
}
export interface IScanner {
    QRCode: string;
    ingredient: IIngredient;
}
export interface IToDoListForReturn {
    total: number;
    doneTotal: number;
    todoTotal: number;
    delayTotal: number;
    percentageOfDone: number;
    data: IToDoList[];
}
