using MatchUp.Business;
using MatchUp.Data;
using Presentation;
using UnityEngine;
using Zenject;

public class MainInstaller : MonoInstaller
{
    [SerializeField] private LevelsData levelsData = null;
    [SerializeField] private ResourcedData resourcedData = null;
    [SerializeField] private WindowManager windowManager = null;
    [SerializeField] private LevelSelectorView levelSelectorViewPrefab = null;
    [SerializeField] private CardView cardViewPrefab = null;

    public override void InstallBindings()
    {
        Container.Bind<ILevelsData>().FromInstance(levelsData).AsSingle();
        Container.Bind<IResourcedData>().FromInstance(resourcedData).AsSingle();
        
        Container.Bind<IGame>().To<Game>().FromResolve();
        Container.Bind<Game>().FromNewComponentOn(gameObject).AsSingle();

        Container.Bind<IGameManager>().To<GameManager>().FromResolve();
        Container.Bind<GameManager>().FromNewComponentOn(gameObject).AsSingle();
        
        Container.Bind<IPlayer>().To<Player>().AsSingle();
        Container.Bind<IGameSerializer>().FromFactory<GameSerializerFactory>().AsSingle();

        Container.Bind<IWindowContainer>().To<WindowContainer>().AsTransient();
        Container.Bind<IWindowManager>().FromInstance(windowManager).AsSingle();

        Container.Bind<ICardViewPresenter>().To<CardViewPresenter>().AsTransient();
        Container.Bind<ILevelSelectorViewPresenter>().To<LevelSelectorViewPresenter>().AsTransient();
        
        Container.Bind<IHomeWindowPresenter>().To<HomeWindowPresenter>().AsTransient();
        Container.Bind<IGameWindowPresenter>().To<GameWindowPresenter>().AsTransient();

        Container.Bind<ICardView>().FromInstance(cardViewPrefab);
        Container.Bind<ICardViewFactory>().To<CardViewFactory>().AsSingle();
        
        Container.Bind<ILevelSelectorView>().FromInstance(levelSelectorViewPrefab);
        Container.Bind<ILevelSelectorViewFactory>().To<LevelSelectorViewFactory>().AsSingle();
    }
}