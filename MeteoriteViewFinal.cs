using UnityEngine;

public class MeteoriteViewFinal : MonoBehaviour
{
    [Header("Efectos")]
    [SerializeField] private ParticleSystem destructionParticlePrefab;
    [SerializeField] private AudioSource destructionSound;

    void Awake()
    {
        //Profesor esto se lo copie a manuel porque no logre que se reproduzca mas de una vez, trate de meterlo dentro del prefab y de todo, al final le copie la idea del tag
        destructionSound = GameObject.FindGameObjectWithTag("MeteoriteSound").GetComponent<AudioSource>();
    }

    public void PlayDestructionEffects()
    {
        if (destructionParticlePrefab != null)
        {
            //no lo use porque me corre el tiempo de entrega, a diferencia del sonido no venian particulas en el proyecto
            ParticleSystem particles = Instantiate(destructionParticlePrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("MeteoriteView: Pariculas no asignadas.", this);
        }

        if (destructionSound != null)
        {
            destructionSound.Play();
            Debug.Log("MeteoriteView: Reproduciendo sonido.", this);
        }
        else
        {
            Debug.LogWarning("MeteoriteView: Sonido no asignado.", this);
        }
    }
}