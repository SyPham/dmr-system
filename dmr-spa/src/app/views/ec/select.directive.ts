import { Directive, AfterViewInit, ElementRef, OnDestroy, OnChanges, HostListener, OnInit } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { IScanner } from 'src/app/_core/_model/IToDoList';

@Directive({
  // tslint:disable-next-line:directive-selector
  selector: '[autoselect]'
})
export class AutoSelectDirective implements AfterViewInit, OnInit, OnDestroy {
  subject = new Subject<string>();
  subscription: Subscription[] = [];
  @HostListener('focus') onFocus() {
    setTimeout( () => {
      this.host.nativeElement.select();
    }, 0);
  }
  @HostListener('ngModelChange', ['$event']) onChange(value) {
    this.subject.next(value);
    // const input = value.split('-') || [];
    // if (input[2]?.length === 8) {
    //     setTimeout(() => {
    //       this.host.nativeElement.select();
    //     }, 0);
    // }
  }
  constructor(private host: ElementRef) { }
  ngAfterViewInit() {
  }
  ngOnInit() {
    this.subscription.push(this.subject
      .pipe(debounceTime(500))
      .subscribe(async (arg) => {
        this.host.nativeElement.select();
      }));
  }
  ngOnDestroy() {
    this.subscription.forEach(item => item.unsubscribe());
  }
}
