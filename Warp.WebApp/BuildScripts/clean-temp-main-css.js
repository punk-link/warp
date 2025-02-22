import fs from 'fs';

if (process.platform === 'win32') {
    try {
        fs.unlinkSync('./Styles/temp-main.css');
        fs.unlinkSync('./Styles/temp-main.css.map');
        console.log('Removed ./Styles/temp-main.css');
    } catch (e) {
        console.log('No file to remove.');
    }
  } else {
        console.log('Not Windows - skipping deletion.');
  }