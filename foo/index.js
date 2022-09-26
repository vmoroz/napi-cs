const addon = require('./bin/aot/Debug/win-x64/publish/module.node')
console.log(addon['hello']);
console.log(addon['plus'](5, 12));
console.log(addon['helloPlusWorld']('hello'));
console.log(addon['add']('hello ')('my world'));

var MyClass = addon.MyClass;
var obj = new MyClass();
obj.print();
obj.printName('world');
console.log(`obj.prop1 = ${obj.prop1}`);
obj.prop1 = 43;
console.log(`obj.prop1 = ${obj.prop1}`);
console.log(`obj.prop2 = ${obj.prop2}`);
obj.prop2 = 23;
console.log(`obj.prop2 = ${obj.prop2}`);