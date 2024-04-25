import * as fs from 'fs'; 
// remove the base URL from the login.js file
{
    const filePath = '../MiniAuth/wwwroot/login.js'; 
    const fileData = fs.readFileSync(filePath, 'utf8');  
    const updatedData = fileData.replace('http://localhost:5566/MiniAuth/', '');  
    fs.writeFileSync(filePath, updatedData, 'utf8');
}
{
    const filePath = '../MiniAuth.IdentityAuth/wwwroot/login.js'; 
    const fileData = fs.readFileSync(filePath, 'utf8');  
    const updatedData = fileData.replace('http://localhost:5566/MiniAuth/', '');  
    fs.writeFileSync(filePath, updatedData, 'utf8');
}