from pathlib import Path
import json
import os

def removeWhitespace(string):
    return ' '.join(string.split())

matList = []
inputPath = r"I:/DMC5/shaders/"
outputPath = r"I:\DMC5\MaterialDump.json"
fileListPath = r"I:\DMC5\dmc5_pak_names_release.list"
fileList = open(fileListPath,'r')
mmtrExt = ".1808168797"

class variableProperty:
    def __init__(self, type, name, size):
        self.vType = type
        self.vName = name
        self.vSize = size

class textureBinding:
    def __init__(self,name):
        self.tName = name
        if name in {"AlphaTranslucentOcclusionSSSMap","Wet_ColormaskWetmaskGradMap","GradationMap"}:
            self.tDefault = "systems/rendering/nullatos.tex"
        elif name in {"NormalRoughnessMap","NormalMap","InnerMaterial_NRMR"}:
            self.tDefault = "systems/rendering/nullnormalroughness.tex"
        elif name == "MajinkaMap":
            self.tDefault = "systems/rendering/nullwhite.tex"
        elif name == "TimeLock_EmissiveColor":
            self.tDefault = "VFX/VFX_Shader/TimeLockEffect_ALB.tex"
        elif name == "Fire_BaseAlphaMap":
            self.tDefault = "VFX/Mesh/MESH_Character/plcommon/mesh_03_plcommon_fire_000_00_00_ALBA.tex"
        else:
            self.tDefault = "systems/rendering/nullblack.tex"

class material:
    def __init__(self, name, fullPath, textureBinds, variableProperties):
        self.mName = name
        self.mFullPath = fullPath
        self.mTexBinds = textureBinds
        self.mVarProps = variableProperties

def to_json(pyObject):
    if isinstance(pyObject,variableProperty):
        if pyObject.vType == "float4":
            return {"name" : pyObject.vName, "type" : pyObject.vType, "size" : pyObject.vSize, "defaultValue" : {"x":1.0,"y":1.0,"z":1.0,"w":1.0}}
        else:
            return {"name" : pyObject.vName, "type" : pyObject.vType, "size" : pyObject.vSize, "defaultValue" : {"data":1.0}}
    elif isinstance(pyObject,material):
        return {"name" : pyObject.mName, "fullPath" : pyObject.mFullPath, "textureBindings" : pyObject.mTexBinds, "variableProperties" : pyObject.mVarProps}
    elif isinstance(pyObject,textureBinding):
        return {"name" : pyObject.tName, "defaultPath" : pyObject.tDefault}
    else:
        print("Object is not JSON-serializable!")

def getFullPath(name):
    for line in fileList:
        if name in line:
            if line.split("/")[-1].split(".")[0] == name:
                return line.replace("natives/x64/","").replace(mmtrExt,"")
    fileList.seek(0)
def readFiles():
    dirs = os.listdir(inputPath)
    for dir in dirs:
        for filePath in Path(inputPath+dir).rglob("*.src"):
            file = filePath.open("r")
            materialName = str(filePath.stem).split("-")[0]
            texList = []
            userMatVariable = []
            print(materialName)
            #print(filePath)
            isKeep = False
            for line in file:
                if "// Resource Bindings:" in line:
                    next(file)
                    header = next(file).replace("\r","").replace("\n","")
                    header = removeWhitespace(header).split(" ")
                    next(file)
                    for line in file:
                        if line.replace("\r","").replace("\n","") == "//":
                            break
                        line = line.replace("\r","").replace("\n","")
                        comment, bname,btype,bformat,bdim,bslot,belements = removeWhitespace(line).split(" ")
                        if bname not in texList and "Srv" not in bname and "SRV" not in bname and btype == "texture":
                            texList.append(textureBinding(bname))
                        if "UserMaterial" in line:
                            isKeep = True #to make sure anything kept does contain cbuffer UserMaterial
                elif "// cbuffer UserMaterial" in line:
                    assert("{" in next(file))
                    next(file)
                    for line in file:
                        if line.replace("\r","").replace("\n","") == "//":
                            break
                        line = line.replace("\r","").replace("\n","")
                        parts = removeWhitespace(line).split(" ")
                        #print(parts)
                        if "//" in parts[2]:
                            parts[2] = parts[2][:-1]
                            parts.insert(3,"//")
                        vtype = parts[1]
                        if "float" not in vtype:
                            print("New variable type: {}.".format(vtype))
                        if "VAR_" in parts[2]:
                            vname = parts[2][4:-1]
                        else:
                            vname = parts[2][:-1]
                        vsize = parts[7]
                        #print("{} {} Size: {}".format(vtype,vname,vsize))
                        if [vtype,vname,vsize] not in userMatVariable and "CAPCOM" not in vname:
                            userMatVariable.append(variableProperty(vtype,vname,vsize))
            if isKeep:
                fullPath = str(getFullPath(filePath.stem.split("-")[0]))
                #print(fullPath)
                newMat = material(materialName,fullPath[:-1],texList,userMatVariable)
                if len(matList) == 0:
                    matList.append(newMat)
                else:
                    isIgnore = False
                    for i in range(len(matList)):
                        if matList[i].mName == newMat.mName:
                            isIgnore = True
                    if not isIgnore:
                        matList.append(newMat)
            file.close()
                

def writeJson():
    outFile = open(outputPath,'w')
    #print(json.dumps(matList,indent=4,default=to_json))
    outFile.write(json.dumps(matList,indent=4,default=to_json))

readFiles()
writeJson()