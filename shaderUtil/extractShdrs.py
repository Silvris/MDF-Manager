from pathlib import Path
import struct
import subprocess
import os

mmtrExt = "*.1808168797"
vsdfExt = "" #doesn't actually exist in DMC5 it seems, but will be useful for MHRise in the future
outputPath = r"I:/DMC5/shaders/"
slimShaderPath = r"I:\MHW\slimshader\SlimShader.Studio.exe"
soloFilePath = r"I:\DMC5\re_chunk_000\natives\x64\mastermaterial\master\character_velvet_fur_8weight.mmtr.1808168797"

def readString(f):
    return ''.join(iter(lambda: f.read(1).decode('ascii'), '\x00'))

def readShdr(file,stem):
    shdrs = []
    assert(file.read(4) == b'SDF\x00')
    groupCount = struct.unpack('H',file.read(2))[0]
    mainCount = struct.unpack('H',file.read(2))[0]
    #print(groupCount,mainCount)
    mainBCOff = file.read(8) #the actual parts use absolute offset, so this is kinda unnecessary
    for i in range(groupCount):
        for j in range(mainCount):
            #print(i,j)
            nameOff = struct.unpack('Q',file.read(8))[0]
            bytecodeOffs = []
            for _ in range(7):
                BCOff = struct.unpack('Q',file.read(8))[0]
                if BCOff != 0:
                    bytecodeOffs.append(BCOff)
            file.read(5*8)
            file.read(5*16)#pairs, latter is a string table offset
            bytecodeSizes = []
            for _ in range(7):
                BCSize = struct.unpack("I",file.read(4))[0]
                if BCSize != 0:
                    bytecodeSizes.append(BCSize)
            file.read(52)#no actual clue what these are
            #print(nameOff)
            shdrs.append([nameOff,i,bytecodeOffs,bytecodeSizes])

    return shdrs
            
def extractShdrs(extension):
    for filePath in Path(r"I:\DMC5\re_chunk_000\natives\x64").rglob(extension):
        shdrfile = open(filePath,'rb')
        print(filePath.stem)
        shdrs = readShdr(shdrfile,filePath.stem)
        #print(shdrs)
        for shdr in shdrs:
            shdrfile.seek(shdr[0])
            #print(shdrfile.tell(),shdr[0])
            shdrName = readString(shdrfile)
            #print(filePath.stem.split(".")[0],shdrName)
            sdfName = filePath.stem.split(".")[0]
            outDirectory = outputPath+"/"+sdfName+"/"
            os.makedirs(outDirectory,exist_ok=True)
            for i in range(len(shdr[2])):
                outPath = outDirectory+sdfName+"-"+str(shdrName)+"-"+str(shdr[1])+"-"+str(i)+".shdr"
                #print(outPath)
                shdrfile.seek(shdr[2][i])
                shdrData = shdrfile.read(shdr[3][i])
                outShdr = open(outPath,'wb')
                outShdr.write(shdrData)
                outShdr.close()
        shdrfile.close()

def convertShdr():
    dirs = os.listdir(outputPath)
    for dir in dirs:
        os.chdir(outputPath+dir)
        subprocess.call([slimShaderPath,"*"])

def soloShdr(soloFile):
    filePath = Path(soloFile)
    shdrfile = open(filePath,'rb')
    print(filePath.stem)
    shdrs = readShdr(shdrfile,filePath.stem)
    #print(shdrs)
    for shdr in shdrs:
        shdrfile.seek(shdr[0])
        #print(shdrfile.tell(),shdr[0])
        shdrName = readString(shdrfile)
        #print(filePath.stem.split(".")[0],shdrName)
        sdfName = filePath.stem.split(".")[0]           
        outDirectory = outputPath+"/"+sdfName+"/"
        os.makedirs(outDirectory,exist_ok=True)
        for i in range(len(shdr[2])):
            outPath = outDirectory+sdfName+"-"+str(shdrName)+"-"+str(shdr[1])+"-"+str(i)+".shdr"
            #print(outPath)
            shdrfile.seek(shdr[2][i])
            shdrData = shdrfile.read(shdr[3][i])
            outShdr = open(outPath,'wb')
            outShdr.write(shdrData)
            outShdr.close()
    shdrfile.close()

#soloShdr(soloFilePath)
extractShdrs(mmtrExt)
convertShdr()