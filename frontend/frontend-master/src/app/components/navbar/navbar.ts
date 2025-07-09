// import { Component } from '@angular/core';

// @Component({
//   selector: 'app-navbar',
//   imports: [],
//   templateUrl: './navbar.html',
//   styleUrl: './navbar.css'
// })
// export class Navbar {

// }

import { Component } from '@angular/core';
import { RouterLink,Router, RouterModule } from '@angular/router';


@Component({
  selector: 'app-navbar',
  imports:[RouterModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css'],
})
export class Navbar {
  isMenuOpen = false;
  constructor(private router: Router) {}

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  onNavItemClick(item: string) {
    console.log(`Navigating to ${item}`);

    // Add your navigation logic here
  }

  onLoginClick() {
    console.log('Login clicked');
    this.router.navigate(['/login']);
    // Add your login logic here
  }
}
