import { Component } from '@angular/core';
import { Registration } from './registration/registration';

@Component({
  selector: 'app-user',
  imports: [Registration],
  templateUrl: './user.html',
  styleUrls: ['./user.css']
})
export class User {

}
