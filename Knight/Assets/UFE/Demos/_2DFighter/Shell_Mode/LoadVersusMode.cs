using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadVersusMode : MonoBehaviour
{
    public UFE3D.GlobalInfo globalConfigFile;
    public UFE3D.CharacterInfo P1SelectedChar;
    public UFE3D.CharacterInfo P2SelectedChar;
    public int selectedStage;
    public string UFESceneName;

    public void LoadUFEScene()
    {
        globalConfigFile.deploymentOptions.deploymentType = UFE3D.DeploymentType.VersusMode;

        globalConfigFile.deploymentOptions.activeCharacters[0] = P1SelectedChar;
        globalConfigFile.deploymentOptions.activeCharacters[1] = P2SelectedChar;
        globalConfigFile.deploymentOptions.AIControlled[0] = false;
        globalConfigFile.deploymentOptions.AIControlled[1] = true;

        globalConfigFile.selectedStage = globalConfigFile.stages[selectedStage];

        SceneManager.LoadScene(UFESceneName);
    }
}