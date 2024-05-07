import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditTermComponent } from './edit-term.component';

describe('EditTermComponent', () => {
  let component: EditTermComponent;
  let fixture: ComponentFixture<EditTermComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditTermComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EditTermComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
