export class HierarchyNode<T> {
    childNodes: Array<HierarchyNode<T>>;
    depth: number;
    hasChildren: boolean;
    parent: T;
    constructor() {
        this.childNodes = new Array<HierarchyNode<T>>();
    }
    any(): boolean {
        return this.childNodes.length > 0;
    }
}
export interface IBuilding {
    id: number;
    level: number;
    name: string;
    parentID: number;
    plans: any;
    settings: any;
}
