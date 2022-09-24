const addon = require('./bin/aot/Debug/win-x64/publish/module.node')
console.log(addon['hello']);
console.log(addon['plus'](5, 12));
console.log(addon['helloPlusWorld']('hello'));
console.log(addon['add']('hello ')('my world'));