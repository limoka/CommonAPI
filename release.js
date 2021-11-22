const fs = require('fs')
const path = require('path')
const archiver = require('archiver');

const STAGE_FOLDER = 'Staging\\';
const ARTIFACT_FOLDER = 'Build\\';
const SRC_FOLDER = 'CommonAPI\\'

const PLUGIN_INFO = 'CommonAPI\\CommonAPIPlugin.cs'
const README = path.join(STAGE_FOLDER, 'README.md');

function main() {
	
	if (process.argv.length >= 4) {
		SRC_FOLDER = process.argv[2];
		PLUGIN_INFO = path.join(SRC_FOLDER, process.argv[3]);
	}
	
	console.log('Buiding artifact, src path: ' + SRC_FOLDER + ', Info file: ' + PLUGIN_INFO);
	
	
    if (!fs.existsSync(STAGE_FOLDER)) {
        fs.mkdirSync(STAGE_FOLDER, {recursive: true})
    }
	if (!fs.existsSync(ARTIFACT_FOLDER)) {
        fs.mkdirSync(ARTIFACT_FOLDER, {recursive: true})
    }
	
	const pluginInfo = getPluginInfo();

    generateManifest(pluginInfo)
	
	var output = fs.createWriteStream(path.join(ARTIFACT_FOLDER, 'CommonAPI Release-' + pluginInfo.version +'.zip'));
	var archive = archiver('zip', {followSymlinks: true});
	
	output.on('close', function () {
		console.log(archive.pointer() + ' total bytes');
		console.log('Success building artifact!');
	});

	archive.on('error', function(err){
		throw err;
	});
	
	archive.pipe(output);

	// append files from a sub-directory, putting its contents at the root of archive
	archive.directory(STAGE_FOLDER, false);

	archive.finalize();
}

function getPluginInfo() {
    const pluginInfoRaw = fs.readFileSync(PLUGIN_INFO).toString("utf-8")
    return {
        name: pluginInfoRaw.match(/ID = "(.*)";/)[1],
        id: pluginInfoRaw.match(/GUID = "(.*)";/)[1],
        version: pluginInfoRaw.match(/VERSION = "(.*)";/)[1],
    }
}

function generateManifest(pluginInfo) {
	
	const manifestPath = path.join(STAGE_FOLDER, 'manifest.json');
	
	let manifest = JSON.parse(fs.readFileSync(manifestPath))
	
	manifest["name"] = pluginInfo.name;
	manifest["version_number"] = pluginInfo.version;
	
    fs.writeFileSync(path.join(STAGE_FOLDER, 'manifest.json'), JSON.stringify(manifest, null, 2))
}

function copyFolderContent(src, dst, excludedExts, excludedNames) {
    fs.readdirSync(src).forEach(file => {
        const srcPath = path.join(src, file)
        const dstPath = path.join(dst, file)
        if (fs.statSync(srcPath).isDirectory() ) {
			if (!excludedNames || !excludedNames.includes(path.basename(srcPath))){
				if (!fs.existsSync(dstPath)) {
					fs.mkdirSync(dstPath)
					copyFolderContent(srcPath, dstPath, excludedExts)
				}
			}
        } else {
            if (!excludedExts || !excludedExts.includes(file.substr(file.lastIndexOf('.')))) {
                fs.copyFileSync(srcPath, dstPath)
            }
        }
    })
}

main();