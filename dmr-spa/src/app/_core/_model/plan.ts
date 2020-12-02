export interface Plan {
    id: number;
    buildingID: number;
    hourlyOutput: number;
    workingHour: number;
    BPFCEstablishID: number;
    BPFCName: string;
    dueDate: any;
}
export interface Consumtion {
    id: number;
    modelName: string;
    modelNo: string;
    articleNo: string;
    process: string;
    glue: string;
    std: number;
    qty: number;
    line: string;
    totalConsumption: number;
    realConsumption: number;
    diff: number;
    percentage: number;
    dueDate: Date;
    mixingDate: Date;
}
export interface Todolist {
    id: number;
    supplier: string;
    glue: string;
    glueID: number;
    lines: string[];
    deliveredActual: string;
    standardConsumption: number;
    status: boolean;
    estimatedTime: Date;
    estimatedStartTime: Date;
    estimatedFinishTime: Date;

}
export interface IDispatch {
    id: number;
    lineID: number;
    line: string;
    standardAmount: number;
    mixingInfoID: number;
    glue: string;
    real: number;
    warningStatus: boolean;
    scanStaus: boolean;
}
export interface IDispatchForCreate {
    id: number;
    lineID: number;
    amount: number;
    mixingInfoID: number;
    createdTime: Date;
    estimatedTime: Date;
}
export interface DispatchParams {
    id: number;
    lines: string[];
    glue: string;
    estimatedTime: Date;
}
export interface IMixingInfo {
    id: number;
    glueID: number;
    glueName: string;
    chemicalA: string;
    chemicalB: string;
    chemicalC: string;
    chemicalD: string;
    chemicalE: string;
    batchA: string;
    batchB: string;
    batchC: string;
    batchD: string;
    batchE: string;
    mixBy: number;
    buildingID: number;
    estimatedTime: any;
    startTime: any;
    endTime: any;
    createdTime: any;
}
