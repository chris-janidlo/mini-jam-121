mkdir -p Assets/Prefabs
mkdir -p Assets/ScriptableObjects/Atoms
mkdir -p Assets/Scripts

rm LICENSE
rm README.md
rm init.sh

git add .
git commit -m "Automated cleanup commit"
git push


if command -v git-lfs; then
    git lfs install
else
    echo "UnityTemplate: git-lfs not initialized due to not being found in path. recommend installing the extension from https://git-lfs.github.com/"
fi
